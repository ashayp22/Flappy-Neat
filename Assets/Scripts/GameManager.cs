using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    //instances/prefabs

    public Bird birdPrefab;
    private List<Bird> birdList;

    public Pipe pipePrefab;
    private List<Pipe> pipeList;

    public Text scoreText;

    public Text playerscoreText;

    public Node nodePrefab;
    public Line linePrefab;

    public GameObject NNCenter;

    public Text nnText;

    private Bird playerBird;
    private bool playerPlaying = false;

    //sprites for the bird + background

    public Sprite[] birdSprites = new Sprite[3];
    public Sprite[] backgroundSprites = new Sprite[2];

    public Sprite playerSprite;

    public GameObject background;

    //settings
    public static Vector2 BIRD_START = new Vector2(-0.35f, 0.5f);

    //NEAT SETTINGS
    public static int YOUNG_BONUS_AGE_THRESHOLD = 10;
    public static double YOUNG_FITNESS_BONUS = 1.2;
    public static int OLD_AGE_THRESHOLD = 50;
    public static double OLD_AGE_PENALTY = 0.7;
    public static int NUM_GENS_ALLOWED_NO_IMPROVEMENT = 3;
    public static int NUM_BIRDS = 20;
    public static double CROSSOVER_RATE = 0.7;
    public static int MAX_PERMITTED_NEURONS = 12;
    public static double CHANCE_ADD_NODE = 0.25;
    public static int NUM_TRYS_TO_FIND_OLD_LINK = 10;
    public static double CHANCE_ADD_LINK = 0.3;
    public static double CHANCE_ADD_RECURRENT_LINK = 0.05;
    public static int NUM_TRYS_TO_FIND_LOOPED_LINK = 0;
    public static int NUM_ADD_LINK_ATTEMPTS = 10;
    public static double MUTATION_RATE = 0.2;
    public static double PROBABILITY_WEIGHT_REPLACED = 0.1;
    public static double MAX_WEIGHT_PERTUBATION = 0.5;
    public static double ACTIVATION_MUTATION_RATE = 0.1;
    public static double MAX_ACTIVATION_PERTUBATION = 0.5;

    public static int[] colorChosen = {204, 0, 0};

    //neat object
    private Cga NEAT;

    private int closePipe1 = -1;
    private int closePipe2 = -1;

    private int score = 0;

    private int bestScore = 0;

    private int bestGeneration = 0;

    private int timeDifference = 155;

    //info for display the Neural Network
    private int selectedBirdNN = 0;

    private bool birdsHidden = false;

    // Start is called before the first frame update
    void Start()
    {
        //creates the pipes
        pipeList = new List<Pipe>();

        Pipe pipe = Instantiate(pipePrefab) as Pipe;
        pipeList.Add(pipe);

        //creates NEAT
        NEAT = new Cga(NUM_BIRDS, 2, 1);

        //creates the birds
        birdList = new List<Bird>();
        for (int i = 0; i < NUM_BIRDS; i++)
        {
            Bird bird = Instantiate(birdPrefab) as Bird;
            birdList.Add(bird);
        }

        //a bird the player can control

        playerBird = Instantiate(birdPrefab) as Bird;
        playerBird.GetComponent<SpriteRenderer>().sprite = playerSprite;
        playerBird.setPlayerBird(); //is a player bird
        hidePlayerBird();
    }


    void Update()
    {
        //checks for removing and adding pipes

        if(!checkRoundOver()) //round isn't over yer
        {

            closePipe1 = closePipe2;

            //goes through movement for each bird

            for(int i = 0; i < NUM_BIRDS; i++)
            {

                if(birdList[i].isDeadBird())
                {
                    continue; //if it is dead, can't move
                }

                //gets the inputs for the bird

                //horizontal distance
                int closestPipe = -1;
                //finds the closest pipe to compare with
                double distance = 10000000;
                for(int z = 0; z < pipeList.Count; z++)
                {
                    double newDistance = pipeList[z].transform.position.x - birdList[z].transform.position.x;
                    if(newDistance >= -0.42 && newDistance < distance) //has to be in front of bird and closer
                    {
                        closestPipe = z;
                        distance = newDistance;
                    }
                }

                closePipe2 = closestPipe;

                if(closestPipe == -1)
                {
                    break;
                }

                //all normalized
                double horizontalDistance = (birdList[i].transform.position.x - pipeList[closestPipe].transform.position.x ) / 2.1;
                double pipeCenter = (pipeList[closestPipe].bottomTop + pipeList[closestPipe].topBottom) / 2;
                double verticalDistance = (pipeCenter - birdList[i].transform.position.y) / 2;

                List<double> inputs = new List<double>();

                inputs.Add(horizontalDistance);
                inputs.Add(verticalDistance);

                if (NEAT.UpdateMember(i, inputs) >= 0.5) //if returns true, move bird up one
                {
                    birdList[i].upBird(); 
                }
            }

            if (closePipe1 == 0 && closePipe2 == 1)
            {
                if(!birdsHidden) //as long as the birds aren't hidden and are playing
                {
                    score++;
                    updateHighScore();
                }
              
                if (playerPlaying) //player is playing
                {
                    Debug.Log("increasing score");
                    playerBird.increasePlayerScore(); //one more point
                }
            }

            //updates the pipes
            if (pipeList[0].transform.position.x < -0.3f && pipeList.Count == 1)
            {
                Pipe pipe = Instantiate(pipePrefab) as Pipe;
                pipeList.Add(pipe);
            }

            if (pipeList[0].transform.position.x < -1.6f)
            {
                //destroy and create new
                Destroy(pipeList[0].gameObject);
                pipeList.RemoveAt(0); //removes the 0 pos
            }

            //updates the scores + times + other
            updateNNdrawn(); //updates the NN drawn
            drawNeuralNetwork(); //draws the NN
            updateBirdScore();
            
            scoreText.text = "GENERATION: " + NEAT.generation + "\nCURRENT SCORE: " + score + "\nBEST SCORE: " + bestScore + "\nBEST GENERATION: " + bestGeneration;
            playerscoreText.text = "PLAYER SCORE: " + playerBird.getPlayerScore();
            //Debug.Log("living");
            nnText.text = "NEURAL NETWORK OF BIRD " + (selectedBirdNN + 1);
        }
        else
        {
            //Debug.Log("dead");
            //have to restart

            //go through neat
            List<double> fitnesses = new List<double>();
            foreach(Bird b in birdList)
            {
                fitnesses.Add(b.getScore()); //adds its s core
                b.resetBird(); //resets the bird
            }


            NEAT.Epoch(fitnesses); //epoch passes

            restart(); //restarts the pipes and other 

        }


        //for skipping 
        if(Input.GetKeyDown(KeyCode.S))
        {
            foreach(Bird b in birdList)
            {
                b.setDead();
            }
        }

        //for player playing


        //for the player playing
        if (playerPlaying && !checkRoundOver())
        {
            Debug.Log("player playing");
            if (playerBird.inLimbo) //just died, needs to reset
            {
                playerBird.transform.position = birdList[selectedBirdNN].transform.position;
                playerBird.inLimbo = false;
                playerBird.resetBird(); //resets it
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerBird.upBird(); //bird goes up
            }
        }


        if (Input.GetKeyDown(KeyCode.P) && !checkRoundOver())
        {
            playerPlaying = !playerPlaying;
            if(playerPlaying)
            {
                showPlayerBird(birdList[selectedBirdNN].transform.position);
            } else
            {
                hidePlayerBird();
            }
        }

        if(Input.GetKeyDown(KeyCode.H) && !checkRoundOver())
        {
            birdsHidden = !birdsHidden;
            if (birdsHidden) //hide the birds
            {
                for(int i = 0; i < birdList.Count; i++)
                {
                    birdList[i].gameObject.SetActive(false);
                }
            }
            else
            {
                //shows the birds playing
                for (int i = 0; i < birdList.Count; i++)
                {
                    if(!birdList[i].isDeadBird()) //currently playing/not dead
                    {
                        birdList[i].gameObject.SetActive(true); 
                    }
                }
            }
        }


    }

    private void updateNNdrawn()
    {
        
        for(int i = 0; i < birdList.Count; i++)
        {
            if(!birdList[i].isDeadBird() && birdList[i].getSelected()) //bird has to be alive and selected
            {
                selectedBirdNN = i;
                break;
            }
        }

        //deselects all birds
        foreach(Bird b in birdList)
        {
            b.notSelected();
        }
    }

    private void updateHighScore()
    {
        if (score > bestScore)
        {
            bestScore = score;
            bestGeneration = NEAT.generation;
        }
    }

    private void updateBirdScore() //updates the score(for each bird)
    {
        foreach(Bird b in birdList)
        {
            if(!b.isDeadBird()) //as long as it is alive
            {

                //gets the closest pipe
                int closestPipe = -1;
                //finds the closest pipe to compare with
                double distance = 10000000;
                for (int z = 0; z < pipeList.Count; z++)
                {
                    double newDistance = pipeList[z].transform.position.x - birdList[z].transform.position.x;
                    if (newDistance >= -0.42 && newDistance < distance) //has to be in front of bird and closer
                    {
                        closestPipe = z;
                        distance = newDistance;
                    }
                }


                if (closestPipe == -1)
                {
                    break;
                }

                //y distance of the pipe

                float yPipe = (pipeList[closestPipe].topBottom + pipeList[closestPipe].bottomTop)/2;

                //y distance of the bird

                float yBird = b.transform.position.y;

                b.updateScore(Mathf.Abs(yPipe - yBird));

            }
        }
    }


    private bool checkRoundOver()
    {
        int dead = 0;
        foreach (Bird b in birdList)
        {
            if (b.isDeadBird())
            {
                dead++;
            }
        }
        return dead == GameManager.NUM_BIRDS;
    }

    private void updateNNColor()
    {
        int randomInt = Random.Range(0, 2);

        if(randomInt == 0)
        {
            colorChosen = new int[3];
            colorChosen[0] = 204;
        } else if(randomInt == 1)
        {
            colorChosen = new int[3];
            colorChosen[0] = 255;
            colorChosen[1] = 204;
        } else
        {
            colorChosen = new int[3];
            colorChosen[0] = 102;
            colorChosen[2] = 204;
        }
    }


    private void restart() //restart the game
    {
       
        updateHighScore();

        score = 0;
        closePipe1 = -1;
        closePipe2 = -1;

        selectedBirdNN = 0;

        foreach (Pipe b in pipeList)
        {
            Destroy(b.gameObject);
        }

        pipeList = new List<Pipe>();

        Pipe pipe = Instantiate(pipePrefab) as Pipe;
        pipeList.Add(pipe);

        int birdSprite = Random.Range(0, birdSprites.Length);

        birdsHidden = false;


        foreach (Bird b in birdList) //each bird is in the starting position
        {
            b.transform.position = BIRD_START;
            b.gameObject.SetActive(true);
            b.GetComponent<SpriteRenderer>().sprite = birdSprites[birdSprite];
        }

        //player bird is reset and is no longer playing
        playerBird.resetBird();
        playerPlaying = false;
        hidePlayerBird();


        background.GetComponent<SpriteRenderer>().sprite = backgroundSprites[Random.Range(0, 2)];
        updateNNColor();

    }

    Vector3 addition = new Vector3(3.0f, 1.5f);

    private void drawNeuralNetwork()
    {
        //choose a bird's nn to display
        int selectedBird = -1;

        if(birdList[selectedBirdNN].isDeadBird()) //the bird selected by the user is dead
        {
            for (int i = 0; i < birdList.Count; i++) //finds a suitable bird that isn't dead
            {
                if (!birdList[i].isDeadBird())
                {
                    selectedBird = i;
                    break;
                }
            }
            selectedBirdNN = selectedBird; //selectedBirdNN is the new selected bird 

        }
        else //the selected bird is the one the user chose
        {
            selectedBird = selectedBirdNN;
        }

        //destroy any children of the gameboject
        foreach (Transform child in NNCenter.transform)
        {
            Destroy(child.gameObject);
        }

        //now display the nn

        //display the nodes

        for (int i = 0; i < NEAT.getNNNodeSize(selectedBird); i++) //for each of the nodes
        {
            //construct the node
            Node n = Instantiate(nodePrefab) as Node;

            int id = NEAT.getNNId(selectedBird, i);

            n.transform.position = NEAT.getNNNodePosFromID(selectedBird, id);
            n.transform.parent = NNCenter.transform;


            //construct its connections

            List<SLink> connectionList = NEAT.getNNConnections(selectedBird, i);

            foreach(SLink link in connectionList) //creates the lines
            {

                Line line = Instantiate(linePrefab) as Line;

                line.Initialize(link.dWeight, n.transform.position, NEAT.getNNNodePosFromID(selectedBird, link.pOut.neuronId));
                line.transform.parent = NNCenter.transform;

                line.transform.position += addition;

            }

            n.transform.position += addition;

        }


    }

    //player bird code

    private void hidePlayerBird()
    {
        playerBird.isDeadBird(); //makes it dead until the player decides to play with it
        playerBird.gameObject.SetActive(false); //made invisible
        playerPlaying = false;
    }

    private void showPlayerBird(Vector2 newPos)
    {
        playerBird.noLongerDead();
        playerBird.gameObject.SetActive(true);
        playerBird.transform.position = newPos;
        playerPlaying = true;
    }

}
