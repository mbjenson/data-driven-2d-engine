# 2d Data-driven game engine
2d game engine driven by a custom entity component system

## Description
This is a 2d, data-driven game engine which runs on top of a custom entity component system that I wrote.
### Purpose
The purpose of the engine is to make creation of 2d games more accessible in monogame. The engine
is made mostly for those who want to create 2d games with retro-style, pixel-perfect graphics but with
more modern lighting and physics.
### Background
I started this project back in march of 2024 with the goal of creating a flexible game engine
that I could use to create my own games. I chose monogame becuase it has a lot of support and documentation.
The framework has support for audio, rendering, files, shaders, and more which makes it easy to get into the
more enjoyable parts of game dev.
### Current
I am no longer actively working on this project and am going to leave it here for anyone to use or
reference if they want help getting started with monogame or game dev in general.

## Engine
## ECS
The ECS uses a bitmask-based system to retrieve entities that have specific types of components. 
The core of the ECS is the entity manager class. This handles retrieval, modification, creation, 
and other operations on entities.
### Systems
The ECS requires systems to be created which operate on entities with a specific set of components.
Each system contains a signature which defines which entities will be retrieved from the Entity
Manager.
### Sample code
The sample code demonstrates how you can use the engine to create entities. There are many sample
systems that exist within the project which interact with the existing components. In the code, for example,
There is a physics system which creates a simulation of 2d physics on entities which have colliders.
