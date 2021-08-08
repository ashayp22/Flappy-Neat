using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bird : MonoBehaviour
{

    private bool isDead;

    //for moving
    private int upward = 0;
    private bool up;

    private int time = 0;

    private double score = 0;

    private int delay = 0;
    private bool addDelay;

    private int upTimes = 0;

    private bool userSelected = false;

    //for player bird
    private bool isPlayerBird = false;
    private int playerScore = 0; //score for the player

    public bool inLimbo = false;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = GameManager.BIRD_START;
        isDead = false;
        up = false;

        this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    
    private void moveBird(float value) //moves the bird
    {
        transform.position = new Vector2(GameManager.BIRD_START.x, transform.position.y + value);
    }

    public void upBird() //bird going up
    {
        if(!up) //as long as the bird is falling(add !delay if want delay)
        {
            addDelay = true; //start the delay
            up = true;
            upTimes++;
        }
    }


    void FixedUpdate()
    {
        if (!isDead) //makes sure it isn't dead
        {
            time++;

            //if there is the need for a delay, increases its timer
            if (addDelay)
            {
                delay++;
            }

            if(delay >= 10) //no more delay, bird can jump
            {
                delay = 0;
                addDelay = false;
            }

            if (up) //goes up
            {
                //move the bird
                moveBird(0.06f);
                upward++;
                //tilt the bird
                if (upward == 7)
                {
                    upward = 0;
                    up = false;
                }
            }
            else //goes down
            {
                //move the bird
                moveBird(-0.035f);
                //tilt the bird

            }

            float currentRotation = transform.rotation.eulerAngles.z;

            if(currentRotation > 180)
            {
                currentRotation -= 360;
            }


            //tilts the bird
            if (up && currentRotation < 35)
            {
                this.gameObject.transform.rotation = Quaternion.Euler(0, 0, currentRotation + 5);
            } else if(currentRotation > -35)
            {
                this.gameObject.transform.rotation = Quaternion.Euler(0, 0, currentRotation - 1);
            }
        }


        if(isPlayerBird)
        {
            Debug.Log(playerScore);
        }

    }

    void OnCollisionEnter2D(Collision2D collision) //collison
    {
        if(time > 10)
        {
            if (!collision.gameObject.tag.Equals("Bird"))
            {
                if (isPlayerBird)
                {
                    playerScore--; //decreases the player score
                    inLimbo = true;
                }
                else
                {
                    isDead = true;
                    gameObject.SetActive(false);
                }
                
            }
        }
    }

    //others
    public void updateScore(double differenceBetweenPipes) //fitness is dependent on how close it is to the pipe
    {
        score += 6 - differenceBetweenPipes;
    }

    public double getScore() //returns the fitness
    {
        return score + (upTimes * 100);
    }

    public bool isDeadBird()
    {
        return isDead;
    }

    //resets the bird
    public void resetBird()
    {
        userSelected = false;
        upTimes = 0;
        addDelay = false;
        delay = 0;
        isDead = false;
        up = false;
        upward = 0;
        time = 0;
        score = 0;
        this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        falling = false;
        inLimbo = false;
    }

    public void setDead()
    {
        isDead = true;
    }

    //other methods used in the intro scene

    private bool falling = false; //if the bird is falling

    public void cancelUp() //cancels going up, will nwo fall
    {
        falling = true;
    }

    public void cancelFalling() //will stop falling
    {
        falling = false;
    }

    public bool getFalling() //returns falling
    {
        return falling;
    }

    //for the user clicking the bird to display the NN'

    public void OnMouseOver()
    {
        Debug.Log("clicked");
        userSelected = true;
    }

    public bool getSelected()
    {
        return userSelected;
    }

    public void notSelected()
    {
        userSelected = false;
    }

    //for the player 

    public void setPlayerBird()
    {
        isPlayerBird = true;
    }

    public void increasePlayerScore()
    {
        playerScore++;
    }

    public void noLongerDead()
    {
        isDead = false;
    }

    public int getPlayerScore()
    {
        return playerScore;
    }

}
