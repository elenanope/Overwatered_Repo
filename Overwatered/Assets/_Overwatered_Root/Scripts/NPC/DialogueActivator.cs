using Unity.Cinemachine;
using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string dialogueText;
        public enum facialExpression { Happy, Neutral, Sad, Surprised, Scared, Angry, Tired };
        [Tooltip("Emotion that the character will show when reading this line.")]
        public facialExpression emotion;//pueden ser serialize?
        public enum characterName { NPC1, NPC2, NPC3, Fisherman, Kid1, Kid2, Kid3 };
        public characterName NPCName;

        [Tooltip("Will this character gift the player sth. when the dialogue ends?")]
        public bool willHandAnObject;

        [Tooltip("The number that differences this object from the rest. Will be checked only when willHandAnObject = true")]
        public int objectType;
    }
    public DialogueLine[] dialogueInfo;
}
