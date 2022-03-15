using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;

    private NeuralNetwork network;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultipler = 0.1f;
    public float fitnessLimit = 1000f;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    [Header("Network Options")]
    public int NumberOf_Layers = 1;
    public int NumberOf_Neurons = 10;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();
    }

    public void ResetWithNetwork(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    private void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    //Whenever the car collides with the wall, this function is called
    private void OnCollisionEnter(Collision collision)
    {
        Death(); //Call the death function of the car controller which calls the death function of the genetic algorithm
    }
    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    //This function moves the car based on the acceleration and turning values
    private Vector3 input;
    public void MoveCar(float v, float h)
    {
        input = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        input = transform.TransformDirection(input);

        transform.position += input;
        transform.eulerAngles += new Vector3(0, (h * 90) * 0.02f, 0);
    }

    //This fumction casts 3 rays in forward, forward-right and forward-left direction and saves the distance values
    //in the sensor variables (divided by 20)
    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;
        
        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            print("A: " + aSensor);
            Debug.DrawLine(r.origin, hit.point, Color.blue);
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            print("B: " + bSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            print("C: " + cSensor);
            Debug.DrawLine(r.origin, hit.point, Color.green);
        }
    }

    //This fumction calculates the fitness of a car based on the distance travelled by the car and the average speed of the car
    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;
        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + ((aSensor + bSensor + cSensor) * sensorMultipler);

        print("Overall Fitness: " + overallFitness);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();   //Very Very Dumb Network
        }

        if (overallFitness >= fitnessLimit)
        {
            Death();  //Good Enough Network, Kill it also LOL ;)
        }
    }

    private void FixedUpdate()
    {
        InputSensors();
        lastPosition = transform.position;

        //Neural Network
        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);
        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;
        CalculateFitness();

        //a = 0;
        //t = 0;
    }
}
