// Description: GameModeGlobal: Access from anywhere to global game info. Selected vehicle IDs, AI names (Ids)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TS.Generics
{
    public class GameModeGlobal : MonoBehaviour
    {
        public static GameModeGlobal    instance = null;
        public List<int>                vehicleIDList = new List<int>();
        public string                   currentSelectedTrack = "";
        public int                      lastSelectedMenuPage = 0;
        public int                      CurrentVehicleCategory = 0;
        public List<int>                categoryAllowedList = new List<int>();

        public List<int>                vehicleNames = new List<int>();

        void Awake()
        {
            #region Create ony one instance of the gameObject in the Hierarchy
            //Check if instance already exists
            if (instance == null)
                //if not, set instance to this
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            if(currentSelectedTrack == "")
                currentSelectedTrack = SceneManager.GetActiveScene().name;

            #endregion
        }

        public void Init(string sData)
        {
            #region 
            if (vehicleNames.Count == 0)
            {
                GenerateNameList();
            } 
            #endregion
        }

        bool TrueFalse(string value)
        {
            #region 
            if (value == "T") return true;
            else return false; 
            #endregion
        }

        public void GenerateNameList()
        {
            #region 
            vehicleNames.Clear();
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            for (var i = 0; i < howManyPlayer; i++)
                vehicleNames.Add(-1 - i);

            List<int> idsAIList = new List<int>();
            int howManyName = DataRef.instance.vehicleGlobalData.aiNamesList.Count;

            bool isRandom = DataRef.instance.vehicleGlobalData.aiNameRandom;

            if (isRandom)
            {
                for (var i = 0; i < howManyName; i++)
                    idsAIList.Add(i);

                for (var i = 0; i < howManyName; i++)
                {
                    int value = UnityEngine.Random.Range(0, idsAIList.Count);
                    vehicleNames.Add(idsAIList[value]);
                    idsAIList.RemoveAt(value);
                }
            }
            else
            {
                for (var i = 0; i < howManyName; i++)
                    vehicleNames.Add(i);
            }   
            #endregion
        }

        public string NameOfTheTrackDependingGameMode()
        {
            #region 
            string trackName = "";
            int CurrentGameMode = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode;
            // Arcade
            if (CurrentGameMode == 0)
            {
                int currentTrack = GameModeArcade.instance.currentSelection;
                TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[currentTrack];

                trackName = LanguageManager.instance.String_ReturnText(trackParams.selectedListMultiLanguage, trackParams.NameIDMultiLanguage);

            }
            // Time Trial
            if (CurrentGameMode == 1)
            {
                int currentTrack = GameModeTimeTrial.instance.currentSelection;
                TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[currentTrack];

                trackName = LanguageManager.instance.String_ReturnText(trackParams.selectedListMultiLanguage, trackParams.NameIDMultiLanguage);

            }
            // Championship
            if (CurrentGameMode == 2)
            {
                //-> Display track Name
                int currentChampionship = GameModeChampionship.instance.currentSelection;
                int currentTrack = GameModeChampionship.instance.currentTrackInTheList;

                int trackID = DataRef.instance.championshipModeData.listOfChampionship[currentChampionship].listTrackParams[currentTrack].TrackID;
                TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[trackID];

                trackName = LanguageManager.instance.String_ReturnText(trackParams.selectedListMultiLanguage, trackParams.NameIDMultiLanguage);
            }

            return trackName; 
            #endregion
        }
    }
}

