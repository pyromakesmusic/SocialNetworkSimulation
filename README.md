# Social Network Simulation (Unity)



## Overview

This project is a graph-based simulation built in Unity where nodes represent agents and edges represent relationships. Each node makes decisions based on weighted objectives, interacts with nearby nodes, and adapts to a spatial resource map.



The system produces emergent behavior such as clustering, competition, and shifting social dynamics.



---



## Features

- Graph-based architecture (nodes + edges)

- Dynamic relationship system (affinity-based interactions)

- Objective-driven agents (greedy, social, ambitious, etc.)

- Spatial resource map influencing behavior

- Movement with inertia, separation, and local influence

- Emergent clustering and territory-like dynamics



---



## Requirements

- Unity (tested on 2022+ recommended)

- No additional dependencies



---



## How to Run

1. Clone the repository:

```bash

git clone https://github.com/pyromakesmusic/SocialNetworkSimulation.git```

2. Open the project in Unity Hub

3. Open the main scene

4. Press Play



## Project Structure

- Assets/Scripts/ – Core simulation logic

- GraphManager – Main simulation loop

- Node / Edge – Data structures

- NodeView / EdgeView – Visual representations

## Simulation Details

- Nodes evaluate actions each tick to maximize objectives

Movement is influenced by:

- Nearby nodes (attraction/repulsion)

- Separation (collision avoidance)

- Terrain (resource gradient)

Resources are affected by:

- Movement cost

- Relationship benefits

- Spatial map multiplier

- Nodes maintain a short-term memory (resourceTrend) that affects behavior

## Future Improvements

- Continuous relationship affinity (replacing discrete edge types)

- More complex objectives and strategies

- Improved visualization and debugging tools

- Performance optimizations for larger graphs

## Notes


This is an experimental simulation focused on emergent behavior rather than strict realism. Many systems are intentionally simple but designed to interact in complex ways.

