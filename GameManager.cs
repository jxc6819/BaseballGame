using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    //Other Scripts and important things
    public NavHub navHub;
    public MovementManager movementManager;
    public GameObject Player;
    private Vector3 playerStartingPos;
    private Quaternion playerStartingRot;
    public UmpireSoundManager umpireSoundManager;
    public BaserunnerManager brm0;
    public BaserunnerManager brm1;
    public BaserunnerManager brm2;
    public BaserunnerManager brm3;
    public BaseManager firstBase;
    public BaseManager secondBase;
    public BaseManager thirdBase;
    public BaseManager homeBase;
    public GameObject[] infielders;
    public GameObject ball;
    //public GameObject glove;
    //public GameObject hand;
    public GameObject ScriptHolder;
    public GameObject bat;
    public GameObject baserunpath;
    public Transform fieldingBallParent;
    private Vector3 playerFieldingLoc;
    public InputActionReference primaryButton;

    //Inning Info
    public int Strikes;
    public int Balls;
    //public int Outs;
    //public bool manOnFirst;
    //public bool manOnSecond;
    //public bool manOnThird;
    public bool ballInPlay;
    public bool ballFielded;
    

    //Game Info
    //public float inning; //1-18
    //public int cpuScore;
    //public int playerScore;
    //public int cpuBattingOrder = 1;
    //public int playerBattingOrder = 1;
    //public int playerBattingPosition = 3;
    //public int playerFieldingPosition = 2; //0 = pitcher  1 = first  2 = second   3 = third   4 = shortstop  5 = left field
                                           //6 = center field   7 = right field  8 = catcher
    public bool playerHome = true;
    public int GameState; //0=batting 1=fielding
    //private bool stopSimming;

    public bool testContinueSim = false;
    public bool playIsDead = true;


    public void Start()
    {
        playerStartingPos = Player.transform.position;
        playerStartingRot = Player.transform.rotation;
        playerFieldingLoc = infielders[GodScript.playerFieldingPosition].GetComponent<InfielderAI>().startingPos;
        foreach (GameObject infielder in infielders)
        {
            PIF_InfielderAI temp = infielder.GetComponent<PIF_InfielderAI>();
            temp.playerPosition = GodScript.playerFieldingPosition;
        }
       
       // startSim();
        
    }

    public bool manOnFirst;
    public bool manOnSecond;
    public bool manOnThird;
    public void Update()
    {
        manOnFirst = GodScript.manOnFirst;
        manOnSecond = GodScript.manOnSecond;
        manOnThird = GodScript.manOnThird;
    }

    private void OnEnable()
    {
        primaryButton.action.started += PrimaryEnabled;
        Debug.Log("Man on first? " + GodScript.manOnFirst);
        if (GodScript.manOnFirst) 
        { 
            brm1.gameObject.SetActive(true);
            secondBase.ForceOut = true;
        }
        if (GodScript.manOnSecond) 
        {
            brm2.gameObject.SetActive(true);
            if (GodScript.manOnFirst) thirdBase.ForceOut = true;
        }
        if (GodScript.manOnThird) 
        { 
            brm3.gameObject.SetActive(true);
            if (GodScript.manOnFirst && GodScript.manOnSecond) homeBase.ForceOut = true;
        }
    }
    private void OnDisable()
    {
        primaryButton.action.started -= PrimaryEnabled;
    }

    private void PrimaryEnabled(InputAction.CallbackContext context)
    {
        Debug.Log("button pressed");
        if (GameState == 0 && playIsDead)
        {
            Debug.Log("button pressed and passed checkpoint");
            ball.transform.parent = null;
            ball.SetActive(true);
            ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            ball.GetComponent<PathFollower>().enabled = true;
            ball.GetComponent<PathFollower>().distanceTravelled = 0;
            playIsDead = false;
        }

    }




    //public void TriggerFieldingScene()
    //{
    //    GameState = 1;
    //    //Change ball
    //    ball.GetComponent<BallCollision>().enabled = false;
    //    ball.GetComponent<PIF_ballManager>().enabled = true;
    //    ball.GetComponent<PlayerThrowManager>().enabled = true;
    //    ball.transform.parent = fieldingBallParent;

    //    //Change Fielders
    //    foreach(GameObject fielder in infielders)
    //    {
    //        fielder.GetComponent<InfielderAI>().enabled = false;
    //        fielder.GetComponent<PIF_InfielderAI>().enabled = true;
    //    }
    //    infielders[playerFieldingPosition].SetActive(false);

    //    //Player Position
    //    Player.transform.position = playerFieldingLoc;
    //    Player.transform.rotation = Quaternion.RotateTowards(Player.transform.rotation, infielders[0].transform.rotation, 1 * Time.deltaTime);

    //    //Script holder
    //    ScriptHolder.GetComponent<PIF_AnimationManager>().enabled = true;
    //    ScriptHolder.GetComponent<MovementManager>().enabled = false;
    //    ScriptHolder.GetComponent<NavHub>().enabled = false;

    //    //Disable: bat, baserunpath
    //    bat.SetActive(false);
    //    baserunpath.SetActive(false);

    //    //Enable: glove, hand
    //    glove.SetActive(true);
    //    baserunpath.SetActive(true);
    //    hand.SetActive(true);

    //    //Baserunners
    //    brm0.gameObject.SetActive(true);
    //    if (manOnFirst) brm1.gameObject.SetActive(true);
    //    else brm1.gameObject.SetActive(false);
    //    if (manOnSecond) brm2.gameObject.SetActive(true);
    //    else brm2.gameObject.SetActive(false);
    //    if (manOnThird) brm3.gameObject.SetActive(true);
    //    else brm3.gameObject.SetActive(false);

    //}

    //public void TriggerBattingScene()
    //{
    //    GameState = 0;
    //    //Change ball
    //    ball.GetComponent<BallCollision>().enabled = true;
    //    ball.GetComponent<PIF_ballManager>().enabled = false;
    //    ball.GetComponent<PlayerThrowManager>().enabled = false;
    //    ball.transform.parent = null;

    //    //Change fielders
    //    foreach(GameObject fielder in infielders)
    //    {
    //        fielder.GetComponent<InfielderAI>().enabled = true;
    //        fielder.GetComponent<PIF_InfielderAI>().enabled = false;
    //    }
    //    infielders[playerFieldingPosition].SetActive(true);

    //    //Player position
    //    Player.transform.position = new Vector3(-0.62f, 0, 0.99f);
    //    Player.transform.rotation = new Quaternion(0, 128.7f, 0, 0);

    //    //Script holder
    //    ScriptHolder.GetComponent<PIF_AnimationManager>().enabled = false;
    //    ScriptHolder.GetComponent<MovementManager>().enabled = true;
    //    ScriptHolder.GetComponent<NavHub>().enabled = true;

    //    //Enable: bat, baserunpath
    //    bat.SetActive(true);
    //    baserunpath.SetActive(true);

    //    //Disable: glove, hand
    //    glove.SetActive(false);
    //    hand.SetActive(false);

    //    //Baserunners
    //    brm0.gameObject.SetActive(false);
    //    if (manOnFirst) brm1.gameObject.SetActive(true);
    //    else brm1.gameObject.SetActive(false);
    //    if (manOnSecond) brm2.gameObject.SetActive(true);
    //    else brm2.gameObject.SetActive(false);
    //    if (manOnThird) brm3.gameObject.SetActive(true);
    //    else brm3.gameObject.SetActive(false);
    //}
    public void PlayEvent_Out()
    {
        Debug.Log("Out!");
        umpireSoundManager.playOutSound();
        Invoke("playDead", 2f);
        GodScript.Outs++;

    }

    public void PlayEvent_Safe(BaseManager _base)
    {
        Debug.Log("Safe!");
        umpireSoundManager.playSafeSound();
        Invoke("playDead", 2f);
    }

    public bool allRunnersStopped()
    {
        if (brm1.isDoneWithPlay() && brm2.isDoneWithPlay() && brm3.isDoneWithPlay()) return true;
        return false;
    }

    public void playDead()
    {
        Debug.Log("loop");
        if (movementManager.stoppedRunning && allRunnersStopped())
        {
            Debug.Log("death");
         
            if (firstBase.Safe)
            {
                Debug.Log("Man on first");
                GodScript.manOnFirst = true;
            }
            else {
                Debug.Log("Taking man off first GM");
                GodScript.manOnFirst = false; }
            if (secondBase.Safe)
            {
                Debug.Log("Man on second");
                GodScript.manOnSecond = true;
            }
            else { GodScript.manOnSecond = false; Debug.Log("Taking man off second GM"); }
            if (thirdBase.Safe)
            {
                Debug.Log("Man on third");
                GodScript.manOnThird = true;
            }
            else GodScript.manOnThird = false;
            playIsDead = true;
            //Debug.Log("gm play dead");
            //ballInPlay = false;
            //movementManager.player.GetComponent<PathFollower>().enabled = false;
            //Player.transform.position = playerStartingPos;
            //Player.transform.rotation = playerStartingRot;//
            //movementManager.enabled = false;
            //navHub.playDead();
            //movementManager.resetPlay();
            ////brm0.resetPlay();
            ////brm1.resetPlay();
            ////brm2.resetPlay();
            ////brm3.resetPlay();
            if (GodScript.GameState == 1)
            {
                GetComponent<PIF_AnimationManager>().resetPlay();
                ball.GetComponent<PIF_ballManager>().resetPlay();
            }
            //SceneManager.LoadSceneAsync("InBetweenAppearances", LoadSceneMode.Single);
        }
        else
        {
            //playIsDead = false;
            Invoke("playDead", 3);
        }


        //baseManager.resetPlay();
    }

    //Return 0 = out
    //Return 1 = single
    //Return 2 = double
    //Return 3 = triple
    //Return 4 = Home Run
    //Return 5 = Walk
    //public int simAtBat(int pos) //Place in batting rode
    //{
    //    int startNum = UnityEngine.Random.Range(0, 1000);
    //    if(pos == 1)
    //    {
    //        if (startNum <= 340)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 457) return 1;
    //            else if (onbaseNum <= 594) return 2;
    //            else if (onbaseNum <= 600) return 3;
    //            else if (onbaseNum <= 696) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 2)
    //    {
    //        if (startNum <= 338)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 433) return 1;
    //            else if (onbaseNum <= 573) return 2;
    //            else if (onbaseNum <= 587) return 3;
    //            else if (onbaseNum <= 694) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 3)
    //    {
    //        if (startNum <= 332)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 415) return 1;
    //            else if (onbaseNum <= 555) return 2;
    //            else if (onbaseNum <= 566) return 3;
    //            else if (onbaseNum <= 679) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 4)
    //    {
    //        if (startNum <= 325)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 421) return 1;
    //            else if (onbaseNum <= 562) return 2;
    //            else if (onbaseNum <= 570) return 3;
    //            else if (onbaseNum <= 691) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 5)
    //    {
    //        if (startNum <= 318)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 425) return 1;
    //            else if (onbaseNum <= 573) return 2;
    //            else if (onbaseNum <= 585) return 3;
    //            else if (onbaseNum <= 696) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 6)
    //    {
    //        if (startNum <= 311)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 457) return 1;
    //            else if (onbaseNum <= 596) return 2;
    //            else if (onbaseNum <= 608) return 3;
    //            else if (onbaseNum <= 707) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 7)
    //    {
    //        if (startNum <= 309)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 472) return 1;
    //            else if (onbaseNum <= 611) return 2;
    //            else if (onbaseNum <= 621) return 3;
    //            else if (onbaseNum <= 715) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else if(pos == 8)
    //    {
    //        if (startNum <= 303)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 464) return 1;
    //            else if (onbaseNum <= 605) return 2;
    //            else if (onbaseNum <= 620) return 3;
    //            else if (onbaseNum <= 702) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //    else
    //    {
    //        if (startNum <= 296)
    //        {
    //            int onbaseNum = UnityEngine.Random.Range(0, 1000);
    //            if (onbaseNum <= 467) return 1;
    //            else if (onbaseNum <= 608) return 2;
    //            else if (onbaseNum <= 623) return 3;
    //            else if (onbaseNum <= 705) return 4;
    //            else return 5;
    //        }
    //        else return 0;
    //    }
    //}


    //public void startSim()
    //{
    //    while (!stopSimming)
    //    {
    //        if (Outs == 3)
    //        {
    //            Outs = 0;
    //            inning += 1;
    //            manOnFirst = false;
    //            manOnSecond = false;
    //            manOnThird = false;
    //        }
    //        if (playerBattingOrder > 9) playerBattingOrder = 1;
    //        if (cpuBattingOrder > 9) cpuBattingOrder = 1;
    //        if (inning == 19)
    //        {
    //            //end game
    //            Debug.Log("Game over. Final Score: " + playerScore + "-" + cpuScore);
    //            break;

    //        }
    //        else if (playerHome)
    //        {
    //            if (inning % 2 == 0)
    //            {
    //                Debug.Log("Simming Player batter #" + playerBattingOrder + " in the " + inning + "th inning");
    //                playerTeamSim();
    //            }
    //            else
    //            {
    //                Debug.Log("Simming CPU batter #" + cpuBattingOrder + " in the " + inning + "th inning");
    //                cpuTeamSim();
    //            }
    //        }
    //        else
    //        {
    //            if (inning % 2 == 0)
    //            {
    //                cpuTeamSim();
    //            }
    //            else
    //            {
    //                playerTeamSim();
    //            }
    //        }
    //    }
    //}

    //public void playerTeamSim()
    //{
    //    if (playerBattingOrder == playerBattingPosition)
    //    {
    //        //send player to bat
    //        stopSimming = true;
    //        Debug.Log("batting opportunity");
    //        TriggerBattingScene();
    //    }
    //    else
    //    {
    //        int simAtBatResult = simAtBat(playerBattingOrder);
    //        if (simAtBatResult == 0)
    //        {
    //            Outs += 1;
    //            Debug.Log("Out!");
    //        }
    //        else if (simAtBatResult == 1)
    //        {
    //            Debug.Log("single");
    //            if (manOnThird)
    //            {
    //                manOnThird = false;
    //                playerScore++;
    //            }
    //            if (manOnSecond)
    //            {
    //                manOnSecond = false;
    //                int runSim = UnityEngine.Random.Range(1, 2);
    //                if (runSim == 1)
    //                {
    //                    playerScore++;
    //                }
    //                else
    //                {
    //                    manOnThird = true;
    //                }
    //            }
    //            if (manOnFirst)
    //            {
    //                manOnFirst = false;
    //                if (manOnThird) manOnSecond = true;
    //                else
    //                {
    //                    int runSim = UnityEngine.Random.Range(1, 4);
    //                    if (runSim == 1) manOnThird = true;
    //                    else manOnSecond = true;
    //                }
    //            }
    //            manOnFirst = true;
    //        }
    //        else if (simAtBatResult == 2)
    //        {
    //            Debug.Log("double");
    //            if (manOnThird)
    //            {
    //                manOnThird = false;
    //                playerScore++;
    //            }
    //            if (manOnSecond)
    //            {
    //                manOnSecond = false;
    //                playerScore++;
    //            }
    //            if (manOnFirst)
    //            {
    //                manOnFirst = false;
    //                int runSim = UnityEngine.Random.Range(1, 4);
    //                if (runSim == 1) playerScore++;
    //                else manOnThird = true;
    //            }
    //            manOnSecond = true;
    //        }
    //        else if (simAtBatResult == 3)
    //        {
    //            Debug.Log("triple");
    //            if (manOnThird)
    //            {
    //                manOnThird = false;
    //                playerScore++;
    //            }
    //            if (manOnSecond)
    //            {
    //                manOnSecond = false;
    //                playerScore++;
    //            }
    //            if (manOnFirst)
    //            {
    //                manOnFirst = false;
    //                playerScore++;
    //            }
    //            playerScore++;
    //        }
    //        else if (simAtBatResult == 4)
    //        {
    //            Debug.Log("homer");
    //            if (manOnThird) playerScore++;
    //            if (manOnSecond) playerScore++;
    //            if (manOnFirst) playerScore++;
    //            manOnFirst = false;
    //            manOnSecond = false;
    //            manOnThird = false;
    //            playerScore++;
    //        }
    //        else //walk
    //        {
    //            Debug.Log("walk");
    //            if (manOnFirst)
    //            {
    //                if (manOnSecond)
    //                {
    //                    if (manOnThird)
    //                    {
    //                        playerScore++;

    //                    }
    //                    else manOnThird = true;
    //                }
    //                else manOnSecond = true;
    //            }
    //            manOnFirst = true;
    //        }
    //    }
    //    playerBattingOrder++;
    //    //startSim();
    //}
    //public void cpuTeamSim() 
    //{
    //    int simAtBatResult = simAtBat(cpuBattingOrder);
    //    cpuBattingOrder++;
    //    if (simAtBatResult == 0)
    //    {
    //        int fieldSim = UnityEngine.Random.Range(1, 12); //refine odds later
    //        Debug.Log("out");
    //        if (fieldSim == 1)
    //        {
    //            stopSimming = true;
    //            TriggerFieldingScene();
    //            Debug.Log("fielding opportnity");
                
    //        }
    //        else Outs += 1;
    //    }
    //    else if (simAtBatResult == 1)
    //    {
    //        Debug.Log("single");
    //        if (manOnThird)
    //        {
    //            manOnThird = false;
    //            cpuScore++;
    //        }
    //        if (manOnSecond)
    //        {
    //            manOnSecond = false;
    //            int runSim = UnityEngine.Random.Range(1, 2);
    //            if (runSim == 1)
    //            {
    //                cpuScore++;
    //            }
    //            else
    //            {
    //                manOnThird = true;
    //            }
    //        }
    //        if (manOnFirst)
    //        {
    //            manOnFirst = false;
    //            if (manOnThird) manOnSecond = true;
    //            else
    //            {
    //                int runSim = UnityEngine.Random.Range(1, 4);
    //                if (runSim == 1) manOnThird = true;
    //                else manOnSecond = true;
    //            }
    //        }
    //        manOnFirst = true;
    //    }
    //    else if (simAtBatResult == 2)
    //    {
    //        Debug.Log("double");
    //        if (manOnThird)
    //        {
    //            manOnThird = false;
    //            cpuScore++;
    //        }
    //        if (manOnSecond)
    //        {
    //            manOnSecond = false;
    //            cpuScore++;
    //        }
    //        if (manOnFirst)
    //        {
    //            manOnFirst = false;
    //            int runSim = UnityEngine.Random.Range(1, 4);
    //            if (runSim == 1) cpuScore++;
    //            else manOnThird = true;
    //        }
    //        manOnSecond = true;
    //    }
    //    else if (simAtBatResult == 3)
    //    {
    //        Debug.Log("triple");
    //        if (manOnThird)
    //        {
    //            manOnThird = false;
    //            cpuScore++;
    //        }
    //        if (manOnSecond)
    //        {
    //            manOnSecond = false;
    //            cpuScore++;
    //        }
    //        if (manOnFirst)
    //        {
    //            manOnFirst = false;
    //            cpuScore++;
    //        }
    //        cpuScore++;
    //    }
    //    else if (simAtBatResult == 4)
    //    {
    //        Debug.Log("homer");
    //        if (manOnThird) cpuScore++;
    //        if (manOnSecond) cpuScore++;
    //        if (manOnFirst) cpuScore++;
    //        manOnFirst = false;
    //        manOnSecond = false;
    //        manOnThird = false;
    //        cpuScore++;
    //    }
    //    else //walk
    //    {
    //        Debug.Log("walk");
    //        if (manOnFirst)
    //        {
    //            if (manOnSecond)
    //            {
    //                if (manOnThird)
    //                {
    //                    cpuScore++;

    //                }
    //                else manOnThird = true;
    //            }
    //            else manOnSecond = true;
    //        }
    //        manOnFirst = true;
    //    }
        
    //}
}
