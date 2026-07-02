// Description: SelectVehicleList. In the MainMenu attached to Grp_Game_CarSelection.
// Generate a list of AIs and Player vehicles used during the race.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class SelectVehicleList : MonoBehaviour
    {
        public int aiUseTheSameVehicleAsPlayer = 100;
        //public bool once = false;
        public void SelectVehicleArcadeTimeTrial()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                GameModeGlobal.instance.vehicleIDList.Clear();
                int howManyVehicle = DataRef.instance.arcadeModeData.howManyVehicleByRace;

                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    var removeVehicleInVersusMode = DataRef.instance.arcadeModeData.removeVehiclesInVersusMode;
                    howManyVehicle -= removeVehicleInVersusMode;
                }


                int HowManyPlayers = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

                //-> Add Player vehicle Prefab ID to cars list
                for (var i = 0; i < HowManyPlayers; i++)
                {
                    GameModeGlobal.instance.vehicleIDList.Add(InfoVehicle.instance.listSelectedVehicles[i]);
                }

                //-> Add AI vehicle Prefab ID to cars list

                List<int> tmpListVehicleAI = listVehicleAI(howManyVehicle);
                for (var i = 0; i < tmpListVehicleAI.Count; i++)
                {
                    GameModeGlobal.instance.vehicleIDList.Add(tmpListVehicleAI[i]);
                }
            }
            //-> Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                GameModeGlobal.instance.vehicleIDList.Clear();
                int HowManyPlayers = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
                //-> Add Player vehicle Prefab ID to cars list
                for (var i = 0; i < HowManyPlayers; i++)
                {
                    GameModeGlobal.instance.vehicleIDList.Add(InfoVehicle.instance.listSelectedVehicles[i]);
                }
            }
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        public void SelectVehicleChampionship()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                GameModeGlobal.instance.vehicleIDList.Clear();
                int howManyVehicle = ReturnMaxVehicleInChampionshipRace();
                int HowManyPlayers = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

                //-> Add Player vehicle Prefab ID to cars list
                for (var i = 0; i < HowManyPlayers; i++)
                {
                    GameModeGlobal.instance.vehicleIDList.Add(InfoVehicle.instance.listSelectedVehicles[i]);
                }

                //-> Add AI vehicle Prefab ID to cars list
                List<int> tmpListVehicleAI = listVehicleAI(howManyVehicle);
                for (var i = 0; i < tmpListVehicleAI.Count; i++)
                {
                    GameModeGlobal.instance.vehicleIDList.Add(tmpListVehicleAI[i]);
                }
            }
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        List<int> listVehicleAI(int _howManyVehicle)
        {
            #region
           List<int> tmpListVehicleAI = new List<int>();

            int howManyVehicleAvailable = DataRef.instance.vehicleGlobalData.carParametersList.Count;
            int howManyVehicleInTheRace = _howManyVehicle;
            int HowManyPlayers = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            //-> Create the AI car list
            int vehicleCategory = GameModeGlobal.instance.CurrentVehicleCategory;
            for (var i = 0; i < howManyVehicleAvailable; i++)
                if (DataRef.instance.vehicleGlobalData.carParametersList[i].vehicleCategory == vehicleCategory)
                    tmpListVehicleAI.Add(i);

            //-> Remove from that list player cars
            List<int> tmpListVehicleWithoutPlayer = new List<int>(tmpListVehicleAI);
              if (tmpListVehicleWithoutPlayer.Count > HowManyPlayers)
              {
                  for (var i = 0; i < HowManyPlayers; i++)
                  {
                      for (var j = 0; j < tmpListVehicleWithoutPlayer.Count; j++)
                      {
                          if (tmpListVehicleWithoutPlayer[j] == InfoVehicle.instance.listSelectedVehicles[i])
                          {
                            tmpListVehicleWithoutPlayer.RemoveAt(j);
                              break;
                          }
                      }
                  }
              }

          

            //-> Randomize the list
            List<int> listRandomized = new List<int>();
            int listSize = tmpListVehicleAI.Count;

            // Case only 1 vehicle in the category
            if(listSize == 1)
            {
                //Debug.Log("One Car in the category");
                int howManyAI = howManyVehicleInTheRace - HowManyPlayers;
                for (var i = 0; i < howManyAI; i++)
                {
                    listRandomized.Add(tmpListVehicleAI[0]);
                }
            }
            // Case more than one vehicle in the category
            else
            {
                //Debug.Log("More than one Car in the category");
                while (listRandomized.Count < howManyVehicleInTheRace - HowManyPlayers)
                {
                    List<int> tmpListVehicleAICopy = new List<int>(tmpListVehicleAI);
                    for (var i = 0; i < listSize; i++)
                    {
                        int rand = UnityEngine.Random.Range(0, tmpListVehicleAICopy.Count);

                        bool isVehicleUsedByPlayer = IsVehicleSelectedByPlayer(rand, HowManyPlayers, tmpListVehicleAICopy);

                        // Check if you can use the same vehicle as the player
                        if (isVehicleUsedByPlayer)
                        {
                            int useTheSameVehicleAsPlayerProba = aiUseTheSameVehicleAsPlayer;
                            int rand2 = UnityEngine.Random.Range(0, 100);

                            if (rand2 >= useTheSameVehicleAsPlayerProba)
                            {
                                int newVehicleIDForAI = UnityEngine.Random.Range(0, tmpListVehicleWithoutPlayer.Count);
                                tmpListVehicleAICopy[rand] = tmpListVehicleWithoutPlayer[newVehicleIDForAI];
                            }
                        }

                        listRandomized.Add(tmpListVehicleAICopy[rand]);

                        //tmpListVehicleAICopy.RemoveAt(rand);

                        if (listRandomized.Count == howManyVehicleInTheRace - HowManyPlayers)
                            break;
                    }
                }
            }
           

            return listRandomized;
            #endregion
        }

        int ReturnMaxVehicleInChampionshipRace()
        {
            #region
            //-> Find max vehicles use in a race during the championship
            int currentChampionship = GameModeChampionship.instance.currentSelection;

            ChampionshipModeData championshipModeData = DataRef.instance.championshipModeData;
            int maxPlayer = 0;

            for (var i = 0; i < championshipModeData.listOfChampionship[currentChampionship].listTrackParams.Count; i++)
            {
                var howManyPlayer = championshipModeData.listOfChampionship[currentChampionship].listTrackParams[i].howManyVehicleByRace;

                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    var removeVehicleInVersusMode = DataRef.instance.championshipModeData.removeVehiclesInVersusMode;
                    howManyPlayer -= removeVehicleInVersusMode;
                }
                if (howManyPlayer > maxPlayer)
                    maxPlayer = howManyPlayer;
            }

            return maxPlayer;
            #endregion
        }

        public void ClearChampionshipPoints()
        {
            #region
            GameModeChampionship.instance.listScore.Clear();
            #endregion
        }

        /* public void Update()
         {
             if (once)
             {
                 SelectVehicleArcadeTimeTrial();
                 once = false;
             }

             if (once)
             {
                 SelectVehicleChampionship();
                once = false;
             }

         }*/

        bool IsVehicleSelectedByPlayer(int vehicleID,int howManyPlayers, List<int> tmpListVehicleAICopy)
        {
            for (var i = 0; i < howManyPlayers; i++)
            {
                if (tmpListVehicleAICopy[vehicleID] == InfoVehicle.instance.listSelectedVehicles[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}