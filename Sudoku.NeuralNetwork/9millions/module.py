import torch
import torch.nn as nn

from base_bayesion_module import *

class SudokuSolver(nn.Module):
    def __init__(self, constraint_mask, n=9, hidden1=128, bayesian=False):
        super(SudokuSolver, self).__init__()
        self.constraint_mask = constraint_mask.unsqueeze(-1).unsqueeze(0)
        self.n = n
        self.hidden1 = hidden1

        # Feature vector is the 3 constraints
        self.input_size = 3 * n
        self.a1 = nn.ReLU()
        if bayesian:
            self.l1 = BayesianLinear(self.input_size,
                            self.hidden1, bias=False)
            self.l2 = BayesianLinear(self.hidden1,
                            n, bias=False)
        else:
            self.l1 = nn.Linear(self.input_size, self.hidden1, bias=False)
            self.l2 = nn.Linear(self.hidden1, int(self.hidden1 / 2), bias=False)
            self.l3 = nn.Linear(int(self.hidden1/2), n, bias=False)
        self.dropout = nn.Dropout(0.5)
        self.softmax = nn.Softmax(dim=1)

    # x is a (batch, n^2, n) tensor
    def forward(self, x, return_orig=False):
        n = self.n
        bts = x.shape[0]
        c = self.constraint_mask
        min_empty = (x.sum(dim=2) == 0).sum(dim=1).max()
        x_pred = x.clone()
        for a in range(min_empty):
            # score empty numbers
            #print(x.view(bts, 1, 1, n * n, n).size(), x.unsqueeze(1).unsqueeze(1).size())
            constraints = (x.unsqueeze(1).unsqueeze(1) * c).sum(dim=3)
            # empty cells
            empty_mask = (x.sum(dim=2) == 0)

            f = constraints.reshape(bts, n * n, 3 * n)
            y_ = self.l1(f[empty_mask])
            y_ = self.a1(y_)
            y_ = self.dropout(y_)
            y_ = self.l2(y_)
            y_ = self.a1(y_)
            y_ = self.dropout(y_)
            y_ = self.l3(y_)
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
            ones = torch.ones(nz.shape[0]).cuda()
            x.index_put_((nz, mmax_, score_pos[nz, mmax_]), ones)
        if return_orig:
            return x
        else:
            return x_pred
