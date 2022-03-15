using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController carController;

    [Header("Genetic Algorithm Parameters")]
    public int initialPopulationCount = 85;

    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("CrossOver Controls")]
    public int bestAgentSelectionCount = 8;
    public int worstAgentSelectionCount = 3;
    public int numberToCrossover = 39;

    private List<int> genePool = new List<int>(); //Contains the networks selected as parents

    private int naturallySelected;
    private NeuralNetwork[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        //Create the population
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[initialPopulationCount];
        FillPopulationWithRandomValues(population, 0);  //Randomizes the initial population
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        carController.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulationCount)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Initialise(carController.NumberOf_Layers, carController.NumberOf_Neurons); //Initialise the Network randomly at staringIndex in the Population
            startingIndex ++;
        }
    }

    public void Death (float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();

        } else
        {
            RePopulate();
        }
    }

    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation(); //Sort Population based on the fitness value

        NeuralNetwork[] newPopulation = PickBestPopulation(); //We have a gene pool and our new generation

        Crossover(newPopulation); //Now we will crossover
        Mutate(newPopulation); //We will mutate

        FillPopulationWithRandomValues(newPopulation, naturallySelected);
        population = newPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();
    }

    private NeuralNetwork[] PickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulationCount];

        //From First Network to the bestAgentSelectionCount
        for (int i = 0; i < bestAgentSelectionCount; i++)
        {
            /************* Elitism *********/
            //If we just initialize the population to the new population, then the population is also changed because of refernce (C# works like that, so we initialise copy)
            newPopulation[naturallySelected] = population[i].InitialiseCopy(carController.NumberOf_Layers, carController.NumberOf_Neurons);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;
            /*******************************/

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        //From Last Network backward to the worstAgentSelectionCount
        for (int i = 0; i < worstAgentSelectionCount; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        //We have created our gene pool and the new population

        return newPopulation;
    }

    private void Crossover(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            //Here we have found 2 indices for the parents randomly from the gene pool

            //Initialise the children
            NeuralNetwork Child1 = new NeuralNetwork();
            NeuralNetwork Child2 = new NeuralNetwork();
            Child1.Initialise(carController.NumberOf_Layers, carController.NumberOf_Neurons);
            Child2.Initialise(carController.NumberOf_Layers, carController.NumberOf_Neurons);
            Child1.fitness = 0;
            Child2.fitness = 0;

            //Here the crossover is happening

            //Here the weights for the children are being selected randomly from the parents
            for (int w = 0; w < Child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }
            }

            //Here the biases for the children are being selected randomly from the parents
            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }

            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private void Mutate(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;
    }

    private void SortPopulation()
    {
        //Bubble Sort based on the fitness value
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }

    }
}
