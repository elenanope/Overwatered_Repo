using Unity.Cinemachine;
using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(4, 6)] public string[] dialogueLines;
        public enum facialExpression { Happy, Neutral, Sad, Surprised, Scared, Angry, Tired };
        [Tooltip("Emotion that the character will show when reading this line.")]
        public facialExpression emotion;//pueden ser serialize?
        public enum characterName { NPC1, NPC2, NPC3, Fisherman, Kid1, Kid2, Kid3 };
        public characterName NPCName;

        [Tooltip("Will this character gift the player sth. when the dialogue ends?")]
        public bool willHandAnObject;

        [Tooltip("The number that differences this object from the rest. Will be checked only when willHandAnObject = true")]
        public int objectType;

        [Tooltip("Will this character offer a chance to play a minigame?")]
        public bool willGame;

        [Header("Options")]
        [Tooltip("Will this sentence offer answers?")]
        public bool multiOption;
        [TextArea(4, 6)] public string[] dialogueOption;

    }
    public DialogueLine[] dialogueInfo;
    //public dialogueLineIndex para marcar por qué número del array dialogueInfo va
    //por si hay pausas como entrar/salir del minijuego
}
