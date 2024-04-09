from timeit import default_timer
import numpy as np

import random
import pygad
import warnings

warnings.filterwarnings("ignore", category=UserWarning)

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


# instance : tuple[tuple] = ( (0,0,0,0,9,4,0,3,0),
#             (0,0,0,5,1,0,0,0,7),
#             (0,8,9,0,0,0,0,4,0),
#             (0,0,0,0,0,0,2,0,8),
#             (0,6,0,2,0,1,0,5,0),
#             (1,0,2,0,0,0,0,0,0),
#             (0,7,0,0,0,0,5,2,0),
#             (9,0,0,0,6,5,0,0,0),
#             (0,4,0,9,7,0,0,0,0))

solution_nb : int = 1

# The fitness function apply a penalty for each duplicate in a row or column
def fitness_func(ga_instance : pygad.GA, solution : np.ndarray , solution_idx : int) -> float:
        fitness = 100
        solution = np.array(solution).reshape(9,9)
        for i in range(9):
                fitness-= 9 - len(np.unique(solution[i, :])) #row
                fitness-= 9 - len(np.unique(solution[:, i])) #column
        return fitness

# The crossover function is merge of blocks of two sudoku puzzles by splitting at a random point
def crossover_func(parents : np.ndarray, offspring_size : tuple , ga_instance : pygad.GA) -> np.ndarray:
    offsprings = []
    blocks = [parent.reshape(3,3,3,3).swapaxes(1,2).reshape(9,3,3) for parent in parents]
    for _ in range(offspring_size[0]):
        offspring = blocks
        crossover_point = random.randint(0, 9)
        offspring = swap_blocks(offspring, crossover_point)
        offsprings.append(offspring[0].reshape(3,3,3,3).swapaxes(1,2).reshape(9,9).flatten())
    return np.array(offsprings)


# the mutation function is a simple swap of two random cells in the same 3x3 grid of the Sudoku puzzle
def mutation_func(offspring : np.ndarray, ga_instance : pygad.GA) -> np.ndarray:
    mutation_probability = 0.9
    fixed_number = np.array(instance).reshape(9,9) != 0 
    num_mutations = int(mutation_probability * offspring.shape[0]) 

    for _ in range(num_mutations):
        idx = np.random.randint(offspring.shape[0])

        grid_x = np.random.choice([0, 3, 6])
        grid_y = np.random.choice([0, 3, 6])

        non_fixed_indices = []
        for i in range(grid_x, grid_x + 3):
            for j in range(grid_y, grid_y + 3):
                if not fixed_number[i, j]:
                    non_fixed_indices.append((i, j))

        if len(non_fixed_indices) < 2:
            continue

        swap_indices = np.random.choice(len(non_fixed_indices), 2, replace=False)
        i1, j1 = non_fixed_indices[swap_indices[0]]
        i2, j2 = non_fixed_indices[swap_indices[1]]

        offspring[idx, i1*9 + j1], offspring[idx, i2*9 + j2] = offspring[idx, i2*9 + j2], offspring[idx, i1*9 + j1]

    return offspring

previous_best_solution = None

solution = None

def on_generation(ga_instance: pygad.GA) -> None:
    global previous_best_solution, fitness_values, lines, solution

    best_solution, best_solution_fitness, best_solution_idx = ga_instance.best_solution()
    print("=====================================================")
    print("Generation #", ga_instance.generations_completed)
    print("Best solution fitness:", best_solution_fitness)
    print("Best solution:\n", np.array(best_solution).reshape(9,9))
    print("Best solution index:", best_solution_idx)

    if previous_best_solution is not None:
        diff = matrix_difference(np.array(previous_best_solution).reshape(9,9), np.array(solution).reshape(9,9))
        if np.all(diff == 0):
            print("The best solution has not changed from the previous generation.")
        else:
            print("The best solution has changed from the previous generation.")
    if best_solution_fitness == 100:
         solution = np.array(best_solution, dtype='int32').reshape(9,9)
    print("=====================================================")

    previous_best_solution = solution
    
# Hyper-Parameters

num_generations = 100 # Nombre de generations
sol_per_pop = 10000  # Nombre de solution par generations 
gene_space = [i for i in range(1, 10)]  # Valeurs que peuvent prendre les genes, ici de 1 -> 9 pour les valeurs possible dans un Sudoku
mutation_percent_genes = 10 # Pourcentage de mutations
crossover_percent = 100 # Pourcentage de crossover

# Create Genetic Algorithm Instance

ga_instance : pygad.GA = pygad.GA(num_generations=num_generations,
        num_parents_mating=2,
        fitness_func=fitness_func,
        gene_space=gene_space,
        initial_population = create_initial_pop(instance,sol_per_pop),
        crossover_type=crossover_func,
        mutation_type=mutation_func,
        on_generation = on_generation,
        stop_criteria = ["saturate_30", "reach_100"],
        parallel_processing=["thread", 8]
        )

# Run Instance:

start = default_timer()

def solveSudoku(instance):
        global solution
        solution_nb = 1
        ga_instance.run()
        best_solution, best_solution_fitness, best_solution_idx = ga_instance.best_solution()
        print(f"Paramètres de la solution # {solution_nb}:\n", np.array(solution).reshape(9,9))
        print(f"Fitness de la solution # {solution_nb}:", best_solution_fitness)
        print(f"Index de la solution # {solution_nb}:", best_solution_idx)
        solution_nb += 1
        return solution

result = solveSudoku(instance)
if result is not None:
    print ("Solution trouv�e")
        
else:
	print ("Aucune solution trouv�e")

execution = default_timer() - start
print("Le temps de r�solution est de : ", execution, " seconds as a floating point value")
