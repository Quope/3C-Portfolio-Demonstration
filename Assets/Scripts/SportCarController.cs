using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class Wheel
{
    public WheelCollider collider;
    public Transform visualWheelTransform;

    public bool isFront; // roues avant ?
    public bool isSteering; // peut tourner ?
    public bool isMotorized; // peut faire avancer le vehicule ? (4 roues motrices / traction / propulsion)

    [HideInInspector] public float rotation;
}

[System.Serializable]
public class Gear
{
    public float maxSpeedKmh;
    public float torque;
}

public class SportCarController : MonoBehaviour
{
    #region SerializeField
    [Header("General")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Basic Movement")]
    [SerializeField] private float maxSpeedKmh = 130;
    [SerializeField] private float driveBackwardSpeed = 300;

    //[Header("Boost")]
    //[SerializeField] private float boostSpeed = 400;
    //[SerializeField] private float boostTime = 5;
    //[SerializeField] private AnimationCurve boostingCurve;

    [Header("Brake")]
    [SerializeField] private float brakeForce = 5000;
    [SerializeField] private float naturalMotorBrakeForce = 2000;
    [SerializeField] private float brakeDrift = 500;
    [SerializeField] private float brakeTime = 1;
    [SerializeField] private AnimationCurve brakeCurve;

    [Header("Gears")]
    [SerializeField] public Gear[] gears;  
    [SerializeField] private float shiftDelay = 0.3f;
    [SerializeField] private float engineBrakeTorque = 300f;

    [Header("Look")]
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sensitivity = 100;

    [Header("Camera")]
    [SerializeField] public CameraFollowCar camFollow;
    [SerializeField] private Volume globalVolume;

    [Header("Wheels")]
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private float wheelAngle = 30;
    [SerializeField] private float turnTime = 0.5f;
    [SerializeField] private float forwardForwardFriction = 3f;
    [SerializeField] private float forwardBackwardFriction = 3f;
    [SerializeField] private float sidewayForwardFriction = 4f;
    [SerializeField] private float sidewayBackwardFriction = 2f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float driftStifness = 0.4f;
    [SerializeField] private float basicDriftStifness = 1.3f;

    [Header("Lights")]
    [SerializeField] private Light lightLBack;
    [SerializeField] private Light lightRBack;
    [SerializeField] private Light lightLFront;
    [SerializeField] private Light lightRFront;
    [SerializeField] private Light spotL;
    [SerializeField] private Light spotR;
    [SerializeField] float baseIntensity = 0.05f;
    [SerializeField] float maxIntensity = 0.15f;

    [Header("Effects")]
    [SerializeField] private float motorBalance = 5;
    [SerializeField] private float motorBalanceTime = 0.05f;
    [SerializeField] private Transform principalMesh;
    [SerializeField] private float driftEffectConditionValue = 2f;
    [SerializeField] private ParticleSystem driftSteam_L;
    [SerializeField] private ParticleSystem driftSteam_R;
    [SerializeField] private ParticleSystem smoke_L;
    [SerializeField] private ParticleSystem smoke_R;

    #endregion

    private Rigidbody rb;
    [HideInInspector] public Vignette vignette;
    [HideInInspector] public ChromaticAberration chromaticAberration;


    #region Timers

    private float motorBalanceTimer;
    private float brakeTimer;
    private float turnTimer;
    private float turnLeftTimer;
    private float turnRightTimer;

    #endregion

    #region SpeedValues

    public float speedKmh;
    public float speedFactor;

    #endregion

    #region BackwardValues

    [HideInInspector] public bool canNaturalBrakeMotor = false;
    private bool canBackward = false;

    #endregion

    #region Wheels

    [HideInInspector] public float curAngle;
    [HideInInspector] public int currentGear = 0;
    private float shiftTimer;
    private float turnValue;
    private bool activateDrift;

    #endregion

    #region Inputs

    [HideInInspector] public InputActionMap actionMap;
    private InputAction driveForwardAction;
    private InputAction driveBackwardAction;
    private InputAction brakeAction;
    private InputAction turnLeftAction;
    private InputAction turnRightAction;
    private InputAction lookAction;
    private InputAction boostAction;
    private InputAction driftAction;
    private InputAction interactAction;

    #endregion

    private void Awake()
    {
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out chromaticAberration);

        actionMap = inputActions.FindActionMap("SportCar");

        driveForwardAction = actionMap.FindAction("DriveForward");
        driveBackwardAction = actionMap.FindAction("DriveBackward");
        brakeAction = actionMap.FindAction("Brake");
        driftAction = actionMap.FindAction("Drift");
        turnLeftAction = actionMap.FindAction("TurnLeft");
        turnRightAction = actionMap.FindAction("TurnRight");
        boostAction = actionMap.FindAction("Boost");
        lookAction = actionMap.FindAction("Look");
        interactAction = actionMap.FindAction("Interact");

        rb = GetComponent<Rigidbody>();

        turnTimer = 0f;

        driftSteam_L.Stop();
        driftSteam_R.Stop();
        smoke_L.Stop();
        smoke_R.Stop();
        lightLBack.intensity = 0f;
        lightRBack.intensity = 0f;
        lightLFront.intensity = 0f;
        lightRFront.intensity = 0f;
        spotL.intensity = 0f;
        spotR.intensity = 0f;
        activateDrift = false;
    }

    private void Start()
    {
        foreach (Wheel wheel in wheels)
        {
            if (wheel.isFront) SetWheelFriction(wheel.collider, forwardForwardFriction, sidewayForwardFriction);
            else SetWheelFriction(wheel.collider, forwardBackwardFriction, sidewayBackwardFriction);
        }
    }

    private void Update()
    {
        speedKmh = Mathf.Abs(rb.linearVelocity.magnitude * 3.6f);
        Debug.Log("Vitesse : " + speedKmh + " kmh");

        DriftActivation();
        ApplicationVisualMovementWheels();
        //MotorVisualEffect();
    }

    private void FixedUpdate()
    {
        BrakeControl();
        TurnControl();
        UpdateGear();
        UpdateLights();
        ForceWheelsApplication();
    }
    private void LateUpdate()
    {
        camFollow.UpdateCarCamera();
    }

    //private void MotorVisualEffect()
    //{
    //    motorBalanceTimer += Time.deltaTime;
    //    if (motorBalanceTimer >= motorBalanceTime * 2) { principalMesh.localRotation = Quaternion.Euler(0, 0, -motorBalance); motorBalanceTimer = 0; }
    //    else if (motorBalanceTimer >= motorBalanceTime) {  principalMesh.localRotation = Quaternion.Euler(0, 0, motorBalance); }
    //}

    private void DriftActivation()
    {
        if (!activateDrift)
        {
            smoke_L.Play(); smoke_R.Play();
            activateDrift = true;
        }

        float lateralSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
        if (lateralSpeed > driftEffectConditionValue) { driftSteam_L.Play(); driftSteam_R.Play(); }
    }

    private void ApplicationVisualMovementWheels()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.rotation += wheel.collider.rpm * 6f * Time.deltaTime;
            wheel.visualWheelTransform.localRotation = Quaternion.Euler(wheel.rotation, wheel.isSteering ? curAngle : 0f, 0f);
        }
    }

    
    private float CurvePattern(float timer, float time, float minValue, float maxValue, AnimationCurve curve)
    {
        // on est dedans
        timer = Mathf.Clamp(timer, 0f, time); // valeur, minimum, maximum
        float a = curve.Evaluate(timer / time);
        return Mathf.Lerp(minValue, maxValue, a);
    }
    
    private void ForceWheelsApplication()
    {
        // Inputs
        float throttleInput = driveForwardAction.IsPressed() ? 1f : 0f;
        float reverseInput = driveBackwardAction.IsPressed() ? -1f : 0f;
        float brakeInput = brakeAction.IsPressed() ? 1f : 0f;
        float driftInput = driftAction.IsPressed() ? 1f : 0f;

        // si shiftTimer > 0f OU speedKmh >= maxSpeedKmg alors on set le torque a 0 pour eviter une autre accélération
        // sinon, on prend le torque lié a la vitesse du rapport actuel qu'on multiplie par la valeur de notre input (1 ou 0)
        float torque = (shiftTimer > 0f || speedKmh >= maxSpeedKmh && !canBackward) ? 0f : gears[currentGear].torque * throttleInput;
        float reverseTorque = driveBackwardSpeed * reverseInput;

        // Frein normal progressif avec la courbe
        float brake = brakeInput * CurvePattern(brakeTimer, brakeTime, 0, brakeForce, brakeCurve);

        // Frein moteur
        if (throttleInput == 0f && brakeInput == 0f && reverseInput == 0f) brake = engineBrakeTorque;

        // Turn Control
        turnTimer = Mathf.Clamp(turnTimer, 0f, turnTime);
        float d = turningCurve.Evaluate(turnTimer / turnTime);
        speedFactor = Mathf.InverseLerp(0f, maxSpeedKmh, speedKmh); // minimum, maximum, value. Sert a savoir où se trouve speedKmh entre 0 et maxSpeedKmh
        float maxAngle = Mathf.Lerp(wheelAngle, wheelAngle * 0.15f, speedFactor); // permet de gérer la rotation des roues en fonction de la vitesse

        if (turnValue == -1) curAngle = Mathf.Lerp(0, -maxAngle, d);
        else curAngle = Mathf.Lerp(0, maxAngle, d);

        float basicWheelStifness = Mathf.Lerp(sidewayBackwardFriction, basicDriftStifness, curAngle);
        float driftWheelStifness = Mathf.Lerp(sidewayBackwardFriction, driftStifness, curAngle);

        // Attribution des différentes forces (avant, arrière et frein) avec modification de la stifness des roues arrière en fonction de l'angle de la voiture
        foreach (Wheel wheel in wheels)
        {
            if (canBackward) wheel.collider.motorTorque = wheel.isMotorized ? reverseTorque : 0f;
            else wheel.collider.motorTorque = wheel.isMotorized ? torque : 0f;

            wheel.collider.brakeTorque = brake;

            if (wheel.isSteering) wheel.collider.steerAngle = curAngle;
            if (!wheel.isFront && driftInput == 0f) { SetWheelFriction(wheel.collider, forwardBackwardFriction, basicWheelStifness); }
            else if (!wheel.isFront && driftInput == 1f) { SetWheelFriction(wheel.collider, forwardBackwardFriction, driftWheelStifness); wheel.collider.brakeTorque = brakeDrift; }
        }
    }

    private void TurnControl()
    {
        // Logique de rotation des roues avant avec retour de force automatique
        turnLeftTimer = Mathf.Clamp(turnLeftTimer, 0f, turnTime);
        turnRightTimer = Mathf.Clamp(turnRightTimer, 0f, turnTime);

        if (turnLeftAction.IsPressed())
        {
            if (turnRightTimer <= 0)
            {
                turnLeftTimer += Time.fixedDeltaTime;
                turnTimer += Time.fixedDeltaTime;
                turnValue = -1;
            }
            else
            {
                turnRightTimer -= Time.fixedDeltaTime;
                turnTimer -= Time.fixedDeltaTime;
            }
        }
        else if (turnRightAction.IsPressed())
        {
            if (turnLeftTimer <= 0)
            {
                turnRightTimer += Time.fixedDeltaTime;
                turnTimer += Time.fixedDeltaTime;
                turnValue = 1;
            }
            else
            {
                turnLeftTimer -= Time.fixedDeltaTime;
                turnTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            turnLeftTimer -= Time.fixedDeltaTime;
            turnTimer -= Time.fixedDeltaTime;
            turnRightTimer -= Time.fixedDeltaTime;
        }
    }

    private void BrakeControl()
    {
        // Frein progressif avec reinitialisation du temps d'acceleration a 0
        if (brakeAction.IsPressed())
        {
            brakeTimer += Time.fixedDeltaTime * 3;
        }
        else
        {
            brakeTimer -= Time.fixedDeltaTime * 6;
            brakeTimer = Mathf.Clamp(brakeTimer, 0, brakeTime);
        }
    }

    private void UpdateGear()
    {
        // Changement de rapport en fonction du temps avec shift timer
        if (shiftTimer > 0f) { shiftTimer -= Time.fixedDeltaTime; return; }

        // on recupere le rapport actuel puis on passe au suivant si la vitesse max du rapport actuel est atteinte
        if (currentGear < gears.Length - 1 && speedKmh > gears[currentGear].maxSpeedKmh)
        {
            currentGear++;
            shiftTimer = shiftDelay;
        }
        else if (currentGear > 0 && speedKmh < gears[currentGear - 1].maxSpeedKmh * 0.8f) // on retrograde par rapport a la vitesse max
        {
            currentGear--;
        }

        // Activation & Desactivation de la marche arriere au point mort
        if (speedKmh >= -1 && speedKmh <= 1 && driveBackwardAction.IsPressed()) { canBackward = true; }
        else if (speedKmh >= -1 && speedKmh <= 1 && driveForwardAction.IsPressed()) { canBackward = false; }
    }

    private void UpdateLights()
    {
        lightLFront.intensity = 0.5f;
        lightRFront.intensity = 0.5f;
        spotL.intensity = 10f;
        spotR.intensity = 10f;
        float intensity = baseIntensity + brakeTimer / 10;
        float finalIntensity = Mathf.Clamp(intensity, baseIntensity, maxIntensity);
        lightLBack.intensity = finalIntensity;
        lightRBack.intensity = finalIntensity;
    }

    private void SetWheelFriction(WheelCollider wheel, float forwardStiffness, float sidewaysStiffness)
    {
        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.stiffness = forwardStiffness;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = sidewaysStiffness;
        wheel.sidewaysFriction = sideways;
    }

    public void NaturalBrakeMotor()
    {
        if (canNaturalBrakeMotor)
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.brakeTorque = naturalMotorBrakeForce;
            }
        }
    }

    public void OffCar()
    {
        smoke_L.Stop(); smoke_R.Stop(); 

        lightLBack.intensity = 0f;
        lightRBack.intensity = 0f;
        lightLFront.intensity = 0f;
        lightRFront.intensity = 0f;
        spotL.intensity = 0f;
        spotR.intensity = 0f;
        activateDrift = false;
    }
}
