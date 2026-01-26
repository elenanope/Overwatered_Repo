using Unity.Cinemachine;
using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        [Tooltip("These will not be randomized")]
        [TextArea(4, 6)] public string[] dialogueLinesDefault;
        //Click and maintain to start breathing in!
        //When you have enough air, release it 
        //Lastly, beware the direction of the wind!!
        public GameObject dialogueMark;
        public bool dialogueStopper;
        //[SerializeField] DialogueActivator dialoguerInfo = null;

        [Tooltip("Mark this if your dialogue appears only when player is in range")]
        public bool areaDialogue;
        public enum facialExpression { Happy, Neutral, Sad, Surprised, Scared, Angry, Tired };
        [Tooltip("Emotion that the character will show when reading this line.")]
        public facialExpression emotion;//pueden ser serialize?
        public bool emotionless;//pueden ser serialize?
        public enum characterName { NPC1, NPC2, NPC3, Fisherman, Kid1, Kid2, Kid3 };
        public characterName NPCName;// a menos que sea una cinemática, poner esto fuera del struc o que salga si seleccionas un bool de differentNPC

        [Tooltip("Will this character gift the player sth. when the dialogue ends?")]
        public bool willHandAnObject;

        [Tooltip("The number that differences this object from the rest. Will be checked only when willHandAnObject = true")]
        public int objectType;

        [Tooltip("Will this character offer a chance to play a minigame?")]
        public bool willGame;
        public int gameScene;

        [Header("Options")]
        [Tooltip("Will this sentence offer answers?")]
        public bool multiOption;
        [TextArea(4, 6)] public string[] dialogueOptions1;
        [TextArea(4, 6)] public string[] dialogueOptions2;

    }
    public int lineToRead; //si tiene que seguir un orden, sino, que desde otro sitio se declare la randomness
    public int activatorReference; //número de este activator en el manager NPC
    [Tooltip("These will be randomized, dialogue lines default will be ignored")]
    public bool isRandom;
    [TextArea(4, 6)] public string[] greetings;
    [TextArea(4, 6)] public string[] funFacts;
    [TextArea(4, 6)] public string[] goodbyes;
    [TextArea(4, 6)] public string[] positive;
    [TextArea(4, 6)] public string[] negative;
    public DialogueLine[] dialogueInfo;
    //public dialogueLineIndex para marcar por qué número del array dialogueInfo va
    //por si hay pausas como entrar/salir del minijuego
}
