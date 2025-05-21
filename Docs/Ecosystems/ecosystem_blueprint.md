# Ecosystem Blueprint

## Biome Definitions
- **Forest**: High resource, moderate competition
- **Desert**: Low resource, high mutation stress
- **Aquatic**: Resource flow dynamics, predation cycles

## Rulesets
1. **Energy Cycle**: Photosynthesis by Plantforms generates 10 energy units per tick.
2. **Mutation Pool**: Mutation chance = baseRate * genome.complexity
3. **Population Cap**: Max population per biome = 1000 entities
4. **Neuromorphic Nodes**: Organoids act as decision hubs, share weight updates via TFQ.

## Evolution Workflow
1. Spawn initial population
2. Simulate for 100 ticks
3. Evaluate fitness, novelty
4. Reward DLXC based on yield_calculator.py output