using UnityEngine;

public class NPCAI : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Talk()
    {
        //se rotará este personaje
        // otra cámara transicionará de la posición de la anterior a un punto medio entre ambos personajes donde puedas ver al npc a la derecha (ej.) y la cámara esté detrás de ti?
        Debug.Log($"Soy {name} y te estoy hablando :)");
    }
}
