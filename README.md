[![dummy](https://github.com/GlynLeine/RealisticPlantSim/workflows/Build%20Action/badge.svg)](https://github.com/GlynLeine/RealisticPlantSim/actions?query=workflow%3ABuild%20Action)

# Project IMT&S: Realistic Plant Simulation
## THE PROBLEM
Simulation can significantly speed up the development of new robots. Simulation can for example be used to test hardware that is physically not available yet. It can also be used to generate sensor data that can be used during development. In this project we primarily focus on generating sensor data.

Eventually, we want to test agricultural robots in simulation. The robots need to recognize plants and pull them out of the ground or harvest them. In this project we focus on the first aspect: having a realistic simulation of plants that move and look realistically. Ideally, these simulations can run in real-time with 100s of plants. In parallel, we need to explore how to combine the simulation in Unity with the robotic simulation in ROS and Gazebo.

## OUR SOLUTION
The Realistic Plant Simulation (RPS) application is built as a suite of editor tools within Unity. These tools offer the possibility to the ROS comunity to test their code within a virtualized environment inside of Unity. This is less time consuming and more graphically realistic than the current ways of testing their code using programs like Gazebo or going out to a field with an actual robot. 

### Manual
For a full manual to our project with instruction on how to set it up and how to use it. Including links to external parts of the project that aren't on this repository. Please refer to the user manual: [User Manual](https://docs.google.com/document/d/1ElO3Z174t5oolri0vyFrG4HCnSy8ZSZZBn0QKAvnmeU/edit?usp=sharing)

### DESIRED OUTCOME
To address all of these issues an application needs to be created that can implement all of these requirements and possibly more in the future. 
This application needs to contain at least the following features:

#### A virtual field of realistic looking plants
To correctly simulate agricultural robots there needs to be a virtual environment for it to be simulated in.
This environment will exist in a virtual farmland field with plants. The field and plants will both be procedurally generated at editor time.
A virtual robot that can move through the environment and be controlled by outside scripts/connections
To do something with the virtual environment there needs to be a robot that can move around the scene and get visual data from it.
This robot must also be able to be controlled by external scripts or connections such as ROS

#### A connection to ROS
The unity project needs to be able to connect to ROS to be able to interface with actual robot software.
It needs to have two way communication to achieve the desired result. Data must be able to be sent and received over the different types of methods ROS supplies.
It must also be possible for the ROS connection to receive video data from Unity. This means color and depth data from 1 or more Unity cameras

#### Modularity
It is important that the application is modular so many different types of robots and connections can be simulated.
Interaction
The robot needs to be able to interact with the scene. The interaction can mean leaving tracks behind in the sand, or interacting with the plants to cut them etc.
This interaction is important for the realism and usability of the project.

##### TOPICS
Realistic simulation, plants, crops, agriculture, robotic simulation, Unity, ROS, Gazebo

##### GUIDANCE
During the process you will be guided by experts from Saxion and Aeres hogeschool. You will work in the Saxion XR Lab in the Saxion Alphons Ariëns building (Ariënsplein).


##### LINKS
[Saxion Mechatronica](https://www.saxion.nl/onderzoek/smart-industry/mechatronica)<br>
