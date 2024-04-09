import torch
import torch.nn as nn
import numpy as np


def create_constraint_mask():
    constraint_mask = torch.zeros((81, 3, 81), dtype=torch.float)
    # row constraints
    for a in range(81):
        r = 9 * (a // 9)
        for b in range(9):
            constraint_mask[a, 0, r + b] = 1

    # column constraints
    for a in range(81):
        c = a % 9
        for b in range(9):
            constraint_mask[a, 1, c + 9 * b] = 1

    # box constraints
    for a in range(81):
        r = a // 9
        c = a % 9
        br = 3 * 9 * (r // 3)
        bc = 3 * (c // 3)
        for b in range(9):
            r = b % 3
            c = 9 * (b // 3)
            constraint_mask[a, 2, br + bc + r + c] = 1

    return constraint_mask

class SudokuSolver(nn.Module):


    def __init__(self, n=9, hidden1=128, bayesian=False):
        super(SudokuSolver, self).__init__()
        self.constraint_mask = create_constraint_mask().unsqueeze(-1).unsqueeze(0)
        self.n = n
        self.hidden1 = hidden1

        # Feature vector is the 3 constraints
        self.input_size = 3 * n
        self.a1 = nn.ReLU()
        self.l1 = nn.Linear(self.input_size,
                            self.hidden1, bias=False)
        self.l2 = nn.Linear(self.hidden1,
                            n, bias=False)
        self.softmax = nn.Softmax(dim=1)

    # x is a (batch, n^2, n) tensor
    def forward(self, x, return_orig=False):
        #logger.debug(f"x shape {x.shape}")
        n = self.n
        bts = x.shape[0]
        c = self.constraint_mask
        min_empty = (x.sum(dim=2) == 0).sum(dim=1).max()
        x_pred = x.clone()
        #logger.debug(f"min empty {min_empty}")

        for a in range(min_empty):
            # score empty numbers
            #print(x.view(bts, 1, 1, n * n, n).size(), x.unsqueeze(1).unsqueeze(1).size())
            constraints = (x.unsqueeze(1).unsqueeze(1) * c).sum(dim=3)
            # empty cells
            empty_mask = (x.sum(dim=2) == 0)

            f = constraints.reshape(bts, n * n, 3 * n)
            y_ = self.l1(f[empty_mask])
            y_ = self.l2(self.a1(y_))

            s_ = self.softmax(y_)

            # Score the rows
            x_pred[empty_mask] = s_

            s = torch.zeros_like(x_pred)
            s[empty_mask] = s_
            # find most probable guess
            score, score_pos = s.max(dim=2)
            mmax = score.max(dim=1)[1]
            # fill it in
            nz = empty_mask.sum(dim=1).nonzero().view(-1)
            mmax_ = mmax[nz]
            ones = torch.ones(nz.shape[0])
            x.index_put_((nz, mmax_, score_pos[nz, mmax_]), ones)
        if return_orig:
            return x
        else:
            return x_pred


if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)


def one_hot_encode(s):
    return torch.tensor(np.eye(9)[s-1] * (s[:, None] > 0), dtype=torch.float, device='cpu')

def ff(s):
    return np.argmax(s , axis = 2) + 1

path = r"..\..\..\..\Sudoku.NeuralNetwork\9millions\model_save\model_epoch_3.pth"
model = SudokuSolver()
model.load_state_dict(torch.load(path, map_location=torch.device('cpu')))
result = ff(model(one_hot_encode(instance.flatten()).unsqueeze(0)).detach().numpy()).reshape(9,9)
result = result.astype(np.int32)
#print(result)
