import numpy as np
import random

def generate_initial_sudoku(instance: tuple[tuple]) -> np.ndarray:
    instance = np.array(instance).flatten()
    for i in range(0, len(instance), 9):
        for j in range(i, i + 9):
            if instance[j] == 0:
                grid_x, grid_y = (j // 27) * 3, ((j % 9) // 3) * 3
                grid = instance[grid_x*9+grid_y : grid_x*9+grid_y+3] 
                grid = np.append(grid, instance[(grid_x+1)*9+grid_y : (grid_x+1)*9+grid_y+3])
                grid = np.append(grid, instance[(grid_x+2)*9+grid_y : (grid_x+2)*9+grid_y+3])
                possible_numbers = [n for n in range(1, 10) if n not in grid]
                
                if not possible_numbers:
                    instance[i:i+9] = [0 if instance[k] == 0 else instance[k] for k in range(i, i+9)]
                    break
                instance[j] = random.choice(possible_numbers)
    return instance

def generate_initial_sudoku_with_mask(instance: tuple[tuple]) -> np.ndarray:
    instance_array = np.array(instance)
    flat_instance = instance_array.flatten()
    
    for i in range(len(flat_instance)):
        mask = generate_mask_from_instance(flat_instance.reshape((9, 9)))
        if flat_instance[i] == 0:
            row, col = i // 9, i % 9
            possible_indices = np.where(mask[row, col])[0] + 1
            if possible_indices.size > 0:
                flat_instance[i] = random.choice(possible_indices)
            else:
                # Choose a random number from 1 to 9 that the block do not containt if no possible numbers are found
                block_row_start = (row // 3) * 3
                block_col_start = (col // 3) * 3
                block = flat_instance[block_row_start*9+block_col_start : block_row_start*9+block_col_start+3]
                block = np.append(block, flat_instance[(block_row_start+1)*9+block_col_start : (block_row_start+1)*9+block_col_start+3])
                block = np.append(block, flat_instance[(block_row_start+2)*9+block_col_start : (block_row_start+2)*9+block_col_start+3])
                possible_indices = [n for n in range(1, 10) if n not in block]
                flat_instance[i] = random.choice(possible_indices)
                

    return flat_instance.reshape((9, 9))


def create_initial_pop(instance: tuple[tuple],pop_size : int) -> np.ndarray:
        population = [np.array(generate_initial_sudoku(instance)).flatten() for _ in range(pop_size)]
        return np.array(population)
    
def create_initial_pop_with_mask(instance: tuple[tuple],pop_size : int) -> np.ndarray:
        population = [np.array(generate_initial_sudoku_with_mask(instance)).flatten() for _ in range(pop_size)]
        return np.array(population)

    
def swap_blocks(offspring: np.ndarray, crossover_point: int) -> np.ndarray:
    for j in range(9):
        if j > crossover_point:
            (offspring[0][j], offspring[1][j]) = (offspring[1][j], offspring[0][j])
    return offspring


def matrix_difference(matrix1: np.ndarray, matrix2: np.ndarray) -> np.ndarray:
    return np.subtract(matrix1, matrix2)


def generate_mask_from_instance(instance: tuple[tuple]) -> np.ndarray:
    mask = np.ones((9, 9, 9), dtype=bool)
    
    instance_array = np.array(instance)
    
    for row in range(9):
        for col in range(9):
            num = instance_array[row, col]
            if num != 0:
                num_index = num - 1
                
                mask[row, col, :] = False
                mask[row, col, num_index] = True
                
                mask[row, :, num_index] = False
                mask[:, col, num_index] = False
                
                block_row_start = (row // 3) * 3
                block_col_start = (col // 3) * 3
                
                mask[block_row_start:block_row_start+3, block_col_start:block_col_start+3, num_index] = False
                
                mask[row, col, num_index] = True
    return mask


# instance = ( (0,0,0,0,9,4,0,3,0),
#             (0,0,0,5,1,0,0,0,7),
#             (0,8,9,0,0,0,0,4,0),
#             (0,0,0,0,0,0,2,0,8),
#             (0,6,0,2,0,1,0,5,0),
#             (1,0,2,0,0,0,0,0,0),
#             (0,7,0,0,0,0,5,2,0),
#             (9,0,0,0,6,5,0,0,0),
#             (0,4,0,9,7,0,0,0,0))

# print(np.array(instance).reshape(9, 9))
# print()
# initial_pop = create_initial_pop_with_mask(instance, 10)
# print(initial_pop[0].reshape(9, 9))

# print(np.array([[1, 2, 3], [4, 5, 6], [7, 8, 9]]).reshape(9, 1))