using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
[RequireComponent (typeof(Rigidbody2D))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("MovimientoJugador")]
    public float velocity;
    [SerializeField] private InputActionReference accionMover;

    [Header("Salto")]
    [SerializeField] private InputActionReference accionSaltar;

    public float fuerzaImpulso = 5f;//fuerza aplicada al saltar
    [SerializeField] LayerMask capaSuelo;

    [Header("Reaparicion")]
    [SerializeField] private Transform spawnPoint;
    private float duracionAparacion = 0.1F;



    

    [Header("Puntuación")]
    [SerializeField] private int puntuacion;


    [Header("Salud")]
    [SerializeField,Min(1)] private int vidas = 3;
    [SerializeField, Min(0.01f)] private float segundosInvulnerables = 1f;
    private bool vulnerable = true;
    private Color colorOriginal;
    public int Vidas => vidas;

    [Header("Tiempo")]
    [SerializeField, Min(1)] private int tiempoLimiteNivel = 100;
    [SerializeField] private int tiempoEmpleado;

    private float tiempoInicio;
    private float tiempoReal;
    public int TiempoEmpleado => tiempoEmpleado;

    [Header("Canvas")]
    [SerializeField] private Canvas canvasInterfaz;
    private ControlDatosJuego controlDatosJuego;

    private ControlHUD hud;

    [Header("Audio")]
    [SerializeField] private AudioClip saltoSonido;
    [SerializeField] private AudioClip vidasSonido;
    private AudioSource efectoSonido;

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
        if(renderizador != null)
        {
            colorOriginal = renderizador.color;
        }

        tiempoInicio = Time.time;

        controlDatosJuego = ControlDatosJuego.Instancia;
        if (controlDatosJuego == null)
        {
            Debug.LogError("Añade Datos Juego al  nivel");
        }
        else
            controlDatosJuego.ReiniciarPartida();

        InicializarHud();

        
    }

    // Update is called once per frame
    void Update()//aqui se lee
    {
        entradaX =accionMover.action.ReadValue<Vector2>();
        SaltoJugador();
        ComprobarFinJuegoTiempo();
        AnimarJugador();
    }

    private void SaltoJugador()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);

        estaTocandoSuelo = hit.collider != null;

        Color colorRayo = estaTocandoSuelo ? Color.green : Color.red;

    Debug.DrawRay(transform.position, Vector2.down* longitudRaycast, colorRayo);

        if (accionSaltar.action.WasPressedThisFrame() && estaTocandoSuelo)
        {
            saltoPendiente = true;
        }
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
            hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);
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
        MoverJugador();
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

    public void Victoria()
    {
        puntuacion = vidas * 100 + Mathf.Max(0, tiempoLimiteNivel - tiempoEmpleado);

        saltoPendiente = false;
        fisica.linearVelocity = Vector2.zero;

        controlDatosJuego.GuardarResultado(true, puntuacion);
        SceneManager.LoadScene("FinNivel");
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

    private void MoverJugador()
    {
        fisica.linearVelocity = new Vector2(entradaX.x * velocity, fisica.linearVelocity.y);

        if (saltoPendiente)
        {
            fisica.AddForce(Vector2.up * fuerzaImpulso, ForceMode2D.Impulse);
        }
        saltoPendiente = false;
    }


    private void ComprobarFinJuegoTiempo()
    {
        tiempoReal = Time.time - tiempoInicio;
        tiempoEmpleado = Mathf.FloorToInt(tiempoReal);
        int segundosRestantes = Mathf.CeilToInt(Mathf.Max(0f, tiempoLimiteNivel - tiempoReal));
        hud.SetTiempo(segundosRestantes);
        hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);

        if(tiempoEmpleado >= tiempoLimiteNivel)
        {
            FinJuego();
            return;
        }
    }

    private void FinJuego()
    {
       if(controlDatosJuego == null)
        {
            Debug.LogError("Falta el objecto Datos Juego para poder guardar el resultado de la partida", this);
            return;
     
        }

        controlDatosJuego.GuardarResultado(false, 0);

        SceneManager.LoadScene("FinNivel");
    }

    public void QuitarVida()
    {
        if (!vulnerable || vidas <= 0) return;
        vulnerable = false;
        vidas--;
        
        hud.SetVidas(vidas);

        if(renderizador != null)
        {
            renderizador.color = Color.blue;
        }
        
        if (vidasSonido != null)
        {
            efectoSonido.PlayOneShot(vidasSonido);
        }

        if(vidas <= 0)
        {
            FinJuego();
            return;
        }

        Invoke(nameof(HacerVulnerable), segundosInvulnerables);//llama al metodo + tiempo
    }


    public void IncrementarPuntos(int cantidad)
    {
        if (cantidad <= 0) return;

        puntuacion += cantidad;
        hud.SetPowerUps(GameObject.FindGameObjectsWithTag("PowerUp").Length);
    }

    private void HacerVulnerable()
    {
        vulnerable = true;

        if (renderizador != null)
        {
            renderizador.color = colorOriginal;
        }
    }

}
