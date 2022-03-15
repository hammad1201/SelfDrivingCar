using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3); //Creating a matrix of 1x3 for the Input Layer

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>(); //Make a list of Matrices for the hidden layers

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2); //Making a matrix for the Output Layer

    public List<Matrix<float>> weights = new List<Matrix<float>>(); //Make a list of Weights for the hidden layers

    public List<float> biases = new List<float>(); //Making a List of float for biases

    public float fitness;

    public void Initialise(int hiddenlayerCount, int hiddenNeuronCount)
    {
        //Clear the vcariables
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        //Initialise the matrices based on the number of layers and neurons

        for (int i = 0; i < hiddenlayerCount + 1; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount); //Make a matrix with the 1 Row and Number of Neurron Coloumns
            hiddenLayers.Add(f);
            biases.Add(Random.Range(-1f, 1f)); //For each f, add a random bias ranging between -1 and 1

            //Weights
            if (i == 0) //First Iteration
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount); //Because 3 inputs and hiddenNeuronCount Coloumns
                weights.Add(inputToH1);
            }

            Matrix<float> hiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(hiddenToHidden);
        }

        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        biases.Add(Random.Range(-1f, 1f));

        //Initialise weight values to random values between -1 and 1
        RandomiseWeights();
    }

    public NeuralNetwork InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        NeuralNetwork n = new NeuralNetwork();

        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }

            newWeights.Add(currentWeight);
        }

        List<float> newBiases = new List<float>();

        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);

        return n;
    }

    public void InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }

    public void RandomiseWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);

                }
            }
        }
    }

    public (float, float) RunNetwork (float a, float b, float c) // Takes sensor values as parameters
    {
        //Initialise the input layer matrix with the sensor values
        inputLayer[0, 0] = a; //1st Row 1st Column
        inputLayer[0, 1] = b; //1st Row 2nd Column
        inputLayer[0, 2] = c; //1st Row 3rd Column

        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        //First output is acceleration and second output is steering
        //Sigmoid gives us values betwenn 0 and 1, while Tanh gives values between -1 and 1
        return (Sigmoid(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
    }


    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }
}
