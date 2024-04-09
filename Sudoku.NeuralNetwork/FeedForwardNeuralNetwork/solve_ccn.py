import os

import numpy as np
from keras import models, utils


class SudokuSolver:
    def __init__(self, model_path):
        self.model = self.load_model(model_path)

    def load_model(self, model_path):
        return models.load_model(model_path)

    def __call__(self, puzzles):
        puzzles = puzzles.copy()
        for _ in range((puzzles == 0).sum((1, 2)).max()):
            model_preds = self.model.predict(
                utils.to_categorical(puzzles, num_classes=10), verbose=0
            )
            preds = np.zeros((puzzles.shape[0], 81, 9))
            for i in range(9):
                for j in range(9):
                    preds[:, i * 9 + j] = model_preds[f"position_{i+1}_{j+1}"]
            probs = preds.max(2)
            values = preds.argmax(2) + 1
            zeros = (puzzles == 0).reshape((puzzles.shape[0], 81))
            for grid, prob, value, zero in zip(puzzles, probs, values, zeros):
                if any(zero):
                    where = np.where(zero)[0]
                    confidence_position = where[prob[zero].argmax()]
                    confidence_value = value[confidence_position]
                    grid.flat[confidence_position] = confidence_value
        return puzzles.squeeze().astype(np.int32)  # Convertir en tableau numpy

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



solver = SudokuSolver("..\..\..\..\Sudoku.NeuralNetwork\FeedForwardNeuralNetwork\cnn.keras")
puzzle = instance[np.newaxis, ...]
result = solver(puzzle)
