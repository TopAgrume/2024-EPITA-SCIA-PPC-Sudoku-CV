import torch
import torch.nn as nn
from torch.utils.data import Dataset, DataLoader
import numpy as np
import pandas as pd

class SudokuDataset(Dataset):
    """Sudoku dataset."""

    def __init__(self, csv_file):
        """
        Args:
            csv_file (string): Path to the csv file with puzzles.
        """
        self.sudoku_frame = pd.read_csv(csv_file)

    def __len__(self):
        return len(self.sudoku_frame)

    def __getitem__(self, idx):
        
        
        x = one_hot_encode(self.sudoku_frame.loc[idx].puzzle)
        y = one_hot_encode(self.sudoku_frame.loc[idx].solution)
        
        
        sample = {'x': x, 'y': y}
        return sample
    
def one_hot_encode(s):
    s = np.array([int(char) for char in s]) - 0 
    return torch.tensor(np.eye(9)[s-1] * (s[:, None] > 0), dtype=torch.float)