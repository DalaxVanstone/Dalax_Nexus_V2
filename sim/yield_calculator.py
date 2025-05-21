import math

def calculate_yield(novelty_score, efficiency, lifespan):
    '''
    Calculate DLXC reward based on:
    - novelty_score: float [0,1]
    - efficiency: float [0,1]
    - lifespan: int (simulation ticks)
    '''
    base = 10
    reward = base * (1 + novelty_score) * efficiency * math.log1p(lifespan)
    return int(reward)

# Example usage
if __name__ == '__main__':
    print(calculate_yield(0.5, 0.8, 100))