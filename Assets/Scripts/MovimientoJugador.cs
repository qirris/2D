using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent (typeof(Rigidbody2D))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("MovimientoJugador")]
    public float velocity;
    [SerializeField] private InputActionReference accionMover;

    [Header("Reaparicion")]
    [SerializeField] private Transform spawnPoint;
    private float duracionAparacion = 0.1F;

    [Header("Canvas")]
    [SerializeField] private Canvas canvasInterfaz;

    private ControlHUD hud;

    [Header("Salto")]
    [SerializeField] private InputActionReference accionSaltar;

    public float fuerzaImpulso = 5f;//fuerza aplicada al saltar
    [SerializeField] LayerMask capaSuelo;
    private Rigidbody2D fisica;
    private SpriteRenderer renderizador;

    private Animator animador;

    private Vector2 entradaX;
    private bool saltoPendiente;

    private bool estaTocandoSuelo;

    private float longitudRaycast = 1.7f;

    private float umbralCarrera = 0.1f;

   

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();

        renderizador = GetComponent<SpriteRenderer>();

        animador = GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {



        InicializarHud();
    }

    // Update is called once per frame
    void Update()//aqui se lee
    {
        entradaX =accionMover.action.ReadValue<Vector2>();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);

        estaTocandoSuelo = hit.collider != null;

        Color colorRayo=estaTocandoSuelo ? Color.green : Color.red;

        Debug.DrawRay(transform.position, Vector2.down * longitudRaycast, colorRayo);

        if (accionSaltar.action.WasPressedThisFrame() && estaTocandoSuelo)
        {
            saltoPendiente = true;
        }
        AnimarJugador();
    }


    private void InicializarHud()
    {
        if (canvasInterfaz != null)
        {
            hud = canvasInterfaz.GetComponent<ControlHUD>();
        }
        else
            Debug.LogError("Asigna el objeto Canvas con el componente ControlHUD al atributo Canvas del jugador");
        if(hud != null)
        {
            hud.SetVidas(vidas);
            hud.SetTiempo(tiempoLimiteNivel);
            hud.SetPowerUps(GameOblect.FindGameObjectWithTag("PowerUp").Length);
        }else
            Debug.LogError("Asigna el objeto Canvas con el componente ControlHUD al atributo Canvas del jugador");
    }
    private void AnimarJugador()
    {
        if (animador == null)
            return;

        if (!estaTocandoSuelo)
        {
            animador.Play("JugadorSaltando");
        }
        else if (Mathf.Abs(fisica.linearVelocity.x) > 0)
        {
            animador.Play("JugadorCorriendo");

        }
        else
        {
            animador.Play("JugadorParado");
        }
    }

    private void OnEnable()//se hace para habilitar
    {
        if ((accionMover == null) || accionMover.action == null)
        {
            Debug.LogError("Asigna Mapa de accion Jugador/Mover en accionMover");
            enabled = false;
            return;
        }

        accionMover.action.Enable();



        if ((accionSaltar == null) || accionSaltar.action == null)
        {
            Debug.LogError("Asigna Mapa de accion Jugador/Saltar en accionSaltar");
            enabled = false;
            return;
        }

        accionSaltar.action.Enable();
    }

    private void OnDisable()//esto es para cuando se quita el gameObject.
    {
        if(accionMover != null && accionMover.action != null)
        {
            accionMover.action.Disable();
        }

        if(accionSaltar != null && accionSaltar.action != null)
        {
            accionSaltar.action.Disable();
        }
    }

    private void FixedUpdate()//siempre que se utilice fisica,se hace aqui/aqui se aplica
    {
        fisica.linearVelocity = new Vector2(entradaX.x * velocity, fisica.linearVelocity.y);

        if (saltoPendiente)
        {
            fisica.AddForce(Vector2.up * fuerzaImpulso,ForceMode2D.Impulse);
        }
        saltoPendiente = false;
        OrientarSprite();
    }

    private void OrientarSprite()
    {
        if (renderizador == null)
            return;
        

        if (fisica.linearVelocity.x > 0)//esto es para que se gire.
        {
            renderizador.flipX = false;
        }
        else if (fisica.linearVelocity.x < 0)
        {
            renderizador.flipX = true;
        }

    }

    public void ReaparecerJugador()
    {
        if (spawnPoint == null)
        {
            Debug.LogError(
                "Asigan un spawn Point al Objecto jugador", this);
            enabled = false;
            return;
        }
        fisica.position = (Vector2)spawnPoint.position;
        fisica.linearVelocity = Vector2.zero;

    }


    private void ComprobarFinJuegoTiempo()
    {
        tiempoReal = Time.time - tiempoInicio;
        tiempoEmpleado = Mathf.FloorToInt(tiempoReal);
        int segundosRestantes = Mathf.CeilToInt(tiempoLimiteNivel - tiempoReal);
        hud.SetTiempo(segundosRestantes);
        if(tiempoEmpleado >= tiempoLimiteNivel)
        {
            FinJuego();
            return;
        }
    }

    public void QuitarVida()
    {
        if(!vulnerable)

        hud.SetVidas(vidas);
    }


    public void IncrementarPuntos(int cantidad)
    {
        if (cantidad <= 0) return;

        puntacion += cantidad;
        hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUP").Length);
    }



    

}
