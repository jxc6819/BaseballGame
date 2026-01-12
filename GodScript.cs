using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GodScript
{
    public static int inning; //1-18
    public static int cpuScore;
    public static int playerScore;
    public static int cpuBattingOrder = 1;
    public static int playerBattingOrder = 1;
    public static int playerBattingPosition = 3;
    public static int playerFieldingPosition = 2;
    public static bool manOnFirst = true;
    public static bool manOnSecond = true;
    public static bool manOnThird;
    public static bool playerHome = true;
    public static bool stopSimming = false;

    public static int Outs = 0;
    public static int GameState = 0;
    

    //TEMPORARY UNTIL MENU SCENE IS CREATED
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RunInitialSim()
    {
        startSim();
    }

    private static void TriggerBattingScene()
    {
        GameState = 0;
        SceneManager.LoadSceneAsync("TestingStartup", LoadSceneMode.Single);
       
    }
    private static void TriggerFieldingScene()
    {
        GameState = 1;
        SceneManager.LoadSceneAsync("FielderTesting", LoadSceneMode.Single);
        
    }

   

    public static void startSim()
    {
        Debug.Log("Sim started");
        while (!stopSimming)
        {
            if (Outs == 3)
            {
                Outs = 0;
                inning += 1;
                manOnFirst = false;
                manOnSecond = false;
                manOnThird = false;
            }
            if (playerBattingOrder > 9) playerBattingOrder = 1;
            if (cpuBattingOrder > 9) cpuBattingOrder = 1;
            if (inning == 19)
            {
                ////end game
                //Debug.Log("Game over. Final Score: " + playerScore + "-" + cpuScore);
                //break;
                inning = 0;
                cpuScore = 0;
                playerScore = 0;
                Outs = 0;
                manOnFirst = false; 
                manOnSecond = false;
                manOnThird = false;

            }
            else if (playerHome)
            {
                if (inning % 2 == 0)
                {
                    Debug.Log("Simming Player batter #" + playerBattingOrder + " in the " + inning + "th inning");
                    playerTeamSim();
                }
                else
                {
                    Debug.Log("Simming CPU batter #" + cpuBattingOrder + " in the " + inning + "th inning");
                    cpuTeamSim();
                }
            }
            else
            {
                if (inning % 2 == 0)
                {
                    cpuTeamSim();
                }
                else
                {
                    playerTeamSim();
                }
            }
        }
    }

    public static void playerTeamSim()
    {
        if (playerBattingOrder == playerBattingPosition)
        {
            //send player to bat
            stopSimming = true;
            Debug.Log("batting opportunity");
            TriggerBattingScene();
        }
        else
        {
            int simAtBatResult = simAtBat(playerBattingOrder);
            if (simAtBatResult == 0)
            {
                Outs += 1;
                Debug.Log("Out!");
            }
            else if (simAtBatResult == 1)
            {
                Debug.Log("single");
                if (manOnThird)
                {
                    manOnThird = false;
                    playerScore++;
                }
                if (manOnSecond)
                {
                    manOnSecond = false;
                    int runSim = UnityEngine.Random.Range(1, 2);
                    if (runSim == 1)
                    {
                        playerScore++;
                    }
                    else
                    {
                        manOnThird = true;
                    }
                }
                if (manOnFirst)
                {
                    manOnFirst = false;
                    if (manOnThird) manOnSecond = true;
                    else
                    {
                        int runSim = UnityEngine.Random.Range(1, 4);
                        if (runSim == 1) manOnThird = true;
                        else manOnSecond = true;
                    }
                }
                manOnFirst = true;
            }
            else if (simAtBatResult == 2)
            {
                Debug.Log("double");
                if (manOnThird)
                {
                    manOnThird = false;
                    playerScore++;
                }
                if (manOnSecond)
                {
                    manOnSecond = false;
                    playerScore++;
                }
                if (manOnFirst)
                {
                    manOnFirst = false;
                    int runSim = UnityEngine.Random.Range(1, 4);
                    if (runSim == 1) playerScore++;
                    else manOnThird = true;
                }
                manOnSecond = true;
            }
            else if (simAtBatResult == 3)
            {
                Debug.Log("triple");
                if (manOnThird)
                {
                    manOnThird = false;
                    playerScore++;
                }
                if (manOnSecond)
                {
                    manOnSecond = false;
                    playerScore++;
                }
                if (manOnFirst)
                {
                    manOnFirst = false;
                    playerScore++;
                }
                playerScore++;
            }
            else if (simAtBatResult == 4)
            {
                Debug.Log("homer");
                if (manOnThird) playerScore++;
                if (manOnSecond) playerScore++;
                if (manOnFirst) playerScore++;
                manOnFirst = false;
                manOnSecond = false;
                manOnThird = false;
                playerScore++;
            }
            else //walk
            {
                Debug.Log("walk");
                if (manOnFirst)
                {
                    if (manOnSecond)
                    {
                        if (manOnThird)
                        {
                            playerScore++;

                        }
                        else manOnThird = true;
                    }
                    else manOnSecond = true;
                }
                manOnFirst = true;
            }
        }
        playerBattingOrder++;
        //startSim();
    }
    public static void cpuTeamSim()
    {
        int simAtBatResult = simAtBat(cpuBattingOrder);
        cpuBattingOrder++;
        if (simAtBatResult == 0)
        {
            Outs += 1;
            //int fieldSim = UnityEngine.Random.Range(1, 12); //refine odds later
            //Debug.Log("out");
            //if (fieldSim == 1)
            //{
            //    stopSimming = true;
            //    TriggerFieldingScene();
            //    Debug.Log("fielding opportnity");

            //}
            //else Outs += 1;
        }
        else if (simAtBatResult == 1)
        {
            Debug.Log("single");
            if (manOnThird)
            {
                manOnThird = false;
                cpuScore++;
            }
            if (manOnSecond)
            {
                manOnSecond = false;
                int runSim = UnityEngine.Random.Range(1, 2);
                if (runSim == 1)
                {
                    cpuScore++;
                }
                else
                {
                    manOnThird = true;
                }
            }
            if (manOnFirst)
            {
                manOnFirst = false;
                if (manOnThird) manOnSecond = true;
                else
                {
                    int runSim = UnityEngine.Random.Range(1, 4);
                    if (runSim == 1) manOnThird = true;
                    else manOnSecond = true;
                }
            }
            manOnFirst = true;
        }
        else if (simAtBatResult == 2)
        {
            Debug.Log("double");
            if (manOnThird)
            {
                manOnThird = false;
                cpuScore++;
            }
            if (manOnSecond)
            {
                manOnSecond = false;
                cpuScore++;
            }
            if (manOnFirst)
            {
                manOnFirst = false;
                int runSim = UnityEngine.Random.Range(1, 4);
                if (runSim == 1) cpuScore++;
                else manOnThird = true;
            }
            manOnSecond = true;
        }
        else if (simAtBatResult == 3)
        {
            Debug.Log("triple");
            if (manOnThird)
            {
                manOnThird = false;
                cpuScore++;
            }
            if (manOnSecond)
            {
                manOnSecond = false;
                cpuScore++;
            }
            if (manOnFirst)
            {
                manOnFirst = false;
                cpuScore++;
            }
            cpuScore++;
        }
        else if (simAtBatResult == 4)
        {
            Debug.Log("homer");
            if (manOnThird) cpuScore++;
            if (manOnSecond) cpuScore++;
            if (manOnFirst) cpuScore++;
            manOnFirst = false;
            manOnSecond = false;
            manOnThird = false;
            cpuScore++;
        }
        else //walk
        {
            Debug.Log("walk");
            if (manOnFirst)
            {
                if (manOnSecond)
                {
                    if (manOnThird)
                    {
                        cpuScore++;

                    }
                    else manOnThird = true;
                }
                else manOnSecond = true;
            }
            manOnFirst = true;
        }

    }

    public static int simAtBat(int pos) //Place in batting rode
    {
        int startNum = UnityEngine.Random.Range(0, 1000);
        if (pos == 1)
        {
            if (startNum <= 340)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 457) return 1;
                else if (onbaseNum <= 594) return 2;
                else if (onbaseNum <= 600) return 3;
                else if (onbaseNum <= 696) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 2)
        {
            if (startNum <= 338)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 433) return 1;
                else if (onbaseNum <= 573) return 2;
                else if (onbaseNum <= 587) return 3;
                else if (onbaseNum <= 694) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 3)
        {
            if (startNum <= 332)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 415) return 1;
                else if (onbaseNum <= 555) return 2;
                else if (onbaseNum <= 566) return 3;
                else if (onbaseNum <= 679) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 4)
        {
            if (startNum <= 325)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 421) return 1;
                else if (onbaseNum <= 562) return 2;
                else if (onbaseNum <= 570) return 3;
                else if (onbaseNum <= 691) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 5)
        {
            if (startNum <= 318)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 425) return 1;
                else if (onbaseNum <= 573) return 2;
                else if (onbaseNum <= 585) return 3;
                else if (onbaseNum <= 696) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 6)
        {
            if (startNum <= 311)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 457) return 1;
                else if (onbaseNum <= 596) return 2;
                else if (onbaseNum <= 608) return 3;
                else if (onbaseNum <= 707) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 7)
        {
            if (startNum <= 309)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 472) return 1;
                else if (onbaseNum <= 611) return 2;
                else if (onbaseNum <= 621) return 3;
                else if (onbaseNum <= 715) return 4;
                else return 5;
            }
            else return 0;
        }
        else if (pos == 8)
        {
            if (startNum <= 303)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 464) return 1;
                else if (onbaseNum <= 605) return 2;
                else if (onbaseNum <= 620) return 3;
                else if (onbaseNum <= 702) return 4;
                else return 5;
            }
            else return 0;
        }
        else
        {
            if (startNum <= 296)
            {
                int onbaseNum = UnityEngine.Random.Range(0, 1000);
                if (onbaseNum <= 467) return 1;
                else if (onbaseNum <= 608) return 2;
                else if (onbaseNum <= 623) return 3;
                else if (onbaseNum <= 705) return 4;
                else return 5;
            }
            else return 0;
        }
    }
}
