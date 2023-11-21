# SudokuSolver

A program that solves Sudoku's for you !

## Description

This application allows you to enter a specifi Sudoku that you'd like to solve an follow step by step the different strategies used to resolve it.
Theses steps are saved as "logs" that you can go back to. They show you the state of the sudoku before the step, after and highlights the board
with the different elements of the strategy. There is also an explanation for each step in the text block below but that is WIP for now. 
All the strategies currently in use are also shown and it is possible to toggle them ON/Off.
The strategies.ini file is essential and dictate multiple characteristics of the strategies (for now : their order; if they are enabled and their behavior on instance found).
You can modify this file however you want to change these characteristics.

Everything is written in C# with dotnet 6.0 and the visuals are done with WPF.

## Features I hope to implement

- Always more strategies
- A new UI look (but I need to learn more about wpf for now)
- A player
- Maybe a generator later in the future

## How to use

If you simply want to use the solver, go to the release section of this Github page select the latest release. Then, download the SudokuSolver.exe
and strateggies.ini files and put them in the same folder. As long as you're on windows, the application should work just fine.

## How to contribute

If you want to contribute to the solver, or atleast use the source code, you'll need dotnet 6.0 and developper tools like Rider or Visual Studio.
