using UnityEngine;

public class ControlJugadorImputSistemAll : MonoBehaviour
{
    public float velocidad;

    private Rigidbody2D fisica;
    private float entradaX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        fisica = GetComponent<Rigidbody2D>(); //Obtiene los valores del rigibody2d del objeto player al inicar la ejecucion.

    }

    // Update is called once per frame
    void Update()
    {

        //Vector2 position = transform.position;

        //position.x = position.x + 0.1f;

        //transform.position = position;


    }

    private void FixedUpdate()
    {
        entradaX = Input.GetAxis("Horizontal");
        if (fisica != null)
        {
            fisica.linearVelocity = new Vector2(entradaX * velocidad, fisica.linearVelocity.y);
        }
        else
            Debug.LogError("El GameObject no tiene asignado un componente RigidbODY2D que es necesario para moverlo.");
    }
}
