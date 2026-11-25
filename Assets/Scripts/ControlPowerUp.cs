using UnityEngine;

public class ControlPowerUp : MonoBehaviour
{
    [SerializeField, Range(1, 10)] private int cantidad = 2;
    [SerializeField] private AudioClip recogerSonido;
    // Start is called once before the first execution of Update after the MonoBehaviour

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (!collision.CompareTag("Player")) return;

        
        if (collision.gameObject.TryGetComponent(out MovimientoJugador jugador))
        {
            jugador.IncrementarPuntos(cantidad);
           

            bool esUltimo = GameObject.FindGameObjectsWithTag("PowerUp").Length == 1;

            AudioSource audioJugador = jugador.GetComponent<AudioSource>();
            if(recogerSonido!=null && audioJugador !=null)
            audioJugador.PlayOneShot(recogerSonido);
            Destroy(gameObject);

            if (esUltimo)
                jugador.Victoria();
        }
        else
        {
            Debug.LogError("El objeto Player necesita un componente movimiento jugador");
        }
    }
}