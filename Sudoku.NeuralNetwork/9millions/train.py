from loguru import logger
import numpy as np
import pandas as pd

import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from tqdm import tqdm

# from base_bayesion_module import *

from dataset import SudokuDataset
from module import SudokuSolver
from utils import *


def train(dataset):
    logger.debug("begin dataset")
    # dataset
    trainloader = DataLoader(SudokuDataset(dataset), batch_size=256,
                                        shuffle=True, num_workers=2)


    constraint_mask = create_constraint_mask().cuda()
    criterion = nn.MSELoss()
    sudoku_solver = SudokuSolver(constraint_mask, hidden1=256, bayesian=False).cuda()

    optimizer = optim.Adam(sudoku_solver.parameters(), lr=0.001)

    #  config
    epochs = 5
    loss_train = []
    loss_val = []
    eval_border = int(len(trainloader) * 0.9)
    logger.debug(f"nombre d'iteration {eval_border}")
    iterations = 0
    save = []
    for e in range(epochs):
        sudoku_solver.train()
        logger.debug(f"debut train epoch {iterations}")
        for i_batch, sudokus in enumerate(trainloader):
            x = sudokus['x'].cuda()
            y = sudokus['y'].cuda()
            if i_batch < eval_border:
                optimizer.zero_grad()
                output = sudoku_solver(x)
                loss = criterion(output, y)
    #             loss = sudoku_solver.sample_elbo(inputs=x,
    #                                    labels=y,
    #                                    criterion=criterion,
    #                                    sample_nbr=100)
                loss.backward()
                optimizer.step()
                loss_train.append(loss.item())
                if iterations % 100 == 0:
                    logger.debug(f"epoch #{e} {iterations} iterations train - {loss.item()}")
                iterations+=1
            else:
                sudoku_solver.eval()
                loss_val.append(evaluate_regression(sudoku_solver, x, y).sum().item())

        logger.debug(f"epoch #{e} {iterations} iterations val - {loss_val[-1]}")
        torch.save(sudoku_solver.state_dict(), f"model_save/model_epochs_drop_{e}.pth")
    torch.save(sudoku_solver.state_dict(), "model_save/model_final_drop.pth")






if __name__ == '__main__':
    logger.add("log/dropout.log")
    logger.debug("start main")
    train(r'D:\epita\ubuntu\SCIA-1\programmationParContrainte\datasets_sudoku\sudoku.csv')