using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoJugadorSaltoMetodo2 : MonoBehaviour
{

    [Header("MovimientoJugador")]
    public float velocity;

    [SerializeField] private InputActionReference accionMover;

    [Header("Salto")]
    [SerializeField] private InputActionReference accionSaltar;

    public float fuerzaImpulso = 5f;//fuerza aplicada al saltar

    [Header("Chequeo de Suelo")]
    [SerializeField] private float distanciaSuelo = 0.2f; //radio esfera
    [SerializeField] private Transform esferaPies;//objeto vacio en los pies
    [SerializeField] LayerMask capaSuelo;


    private Rigidbody2D fisica;
    private Vector2 entradaMovimiento;
    private bool estaTocandoSuelo;
    private bool saltoPendiente;

    private void Awake()
    {
        fisica = GetComponent<Rigidbody2D>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        entradaMovimiento = accionMover.action.ReadValue<Vector2>();

        estaTocandoSuelo=Physics2D.OverlapCircle(esferaPies.position, distanciaSuelo, capaSuelo);

        if(accionSaltar.action.WasPressedThisFrame() && estaTocandoSuelo){
            
            saltoPendiente = true;
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

    private void FixedUpdate()//siempre que se utilice fisica,se hace aqui/aqui se aplica
    {
        fisica.linearVelocity = new Vector2(entradaMovimiento.x*velocity,fisica.linearVelocity.y);

        if (saltoPendiente)
        {
            fisica.AddForce(Vector2.up * fuerzaImpulso, ForceMode2D.Impulse);
        }
        saltoPendiente = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (esferaPies != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(esferaPies.position, distanciaSuelo);
        }
    }

    private void OnDisable()//esto es para cuando se quita el gameObject.
    {
        if (accionMover != null && accionMover.action != null)
        {
            accionMover.action.Disable();
        }

        if (accionSaltar != null && accionSaltar.action != null)
        {
            accionSaltar.action.Disable();
        }
    }
}
