using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ashsvp
{
    [RequireComponent(typeof(SystemGear))]
    public class SimcadeVehicleController : MonoBehaviour
    {
        // сейчас не понятно почему но без этого offset mesh подлетает ввеврх по y на 0.9.
        // что бы понять как это исправить на до покопатся в классе SimcadeVehicleController
        // но он так плохо написан что гораздо легче в будуюшем просто сделать системку которая
        // будет пробегатся по всем машинам и пременять ко всем BODY_OFFSET

        //В `_bodyMesh` хранятся все меши, относящиеся к машине.Я так поступил, поскольку в тестовой
        //сцене контроллер машины настроен именно таким образом, и `SimcadeVehicleController` при этом функционирует.
        //Сам `SimcadeVehicleController` написан очень плохо; по-хорошему, его следовало бы декомпозировать
        //на отдельные классы и добавить инкапсуляцию, и тогда ситуация станет более приемлемой.

        private const float BODY_OFFSET = -0.9f;

        [Header("Suspension")]
        [Space(10)]
        public float springForce = 30000f;
        public float springDamper = 200f;
        private float MaxSpringDistance;

        [Header("Car Stats")]
        [Space(10)]
        
        private Rigidbody rb;

        public float MaxSpeed = 200f;
        public float Acceleration;
        public AnimationCurve AccelerationCurve;
        public float MaxTurnAngle = 30f;
        public AnimationCurve turnCurve;
        public float brakeAcceleration = 50f;
        public float RollingResistance = 2f;
        public float driftFactor = 0.2f;
        public float FrictionCoefficient = 1f;
        public AnimationCurve sideFrictionCurve;
        public AnimationCurve forwardFrictionCurve;
        public Transform CenterOfMass_air;
        private Vector3 CentreOfMass_ground;
        public bool AutoCounterSteer = false;
        public float DownForce = 5;


        [Header("Visuals")]
        [Space(10)]
        public Transform VehicleBody;
        [Range(0, 10)]
        public float forwardBodyTilt = 3f;
        [Range(0, 10)]
        public float sidewaysBodyTilt = 3f;
        public GameObject WheelSkid;
        public GameObject SkidMarkController;
        public float wheelRadius;
        public float skidmarkWidth;
        public Transform[] HardPoints = new Transform[4];
        public Transform[] Wheels;

        [HideInInspector]
        public Vector3 carVelocity;

        [Header("Events")]
        [Space(10)]

        public readonly Vehicle_Events VehicleEvents = new();

        [Serializable]
        public class Vehicle_Events
        {
            public UnityEvent OnTakeOff;
            public UnityEvent OnGrounded;
        }

        private bool tempGroundedProperty;

        [Header("Other Things")]

        private RaycastHit[] wheelHits = new RaycastHit[4];

        [HideInInspector]
        private float steerInput, _brakeInput, rearTrack, wheelBase, ackermennLeftAngle, ackermennRightAngle;

        [HideInInspector]
        public Vector3 localVehicleVelocity;
        private Vector3 lastVelocity;
        private int NumberOfGroundedWheels;
        [HideInInspector]
        public bool vehicleIsGrounded;

        private float[] offset_Prev = new float[4];

        //Skidmarks
        [HideInInspector]
        public float[] forwardSlip = new float[4], slipCoeff = new float[4], skidTotal = new float[4];
        private WheelSkid[] wheelSkids = new WheelSkid[4];

        //NewVariables

        [SerializeField] private GameObject _bodyMesh;

        public float AccelerationInput { get; private set; }

        private Vector3 PositonOffset
        => new Vector3(_bodyMesh.transform.position.x, _bodyMesh.transform.position.y + BODY_OFFSET, _bodyMesh.transform.position.z);

        private void Awake()
        {
            GameObject SkidMarkController_Self = Instantiate(SkidMarkController);
            SkidMarkController_Self.GetComponent<Skidmarks>().SkidmarkWidth = skidmarkWidth;

            rb = GetComponent<Rigidbody>();
            lastVelocity = Vector3.zero;

            for (int i = 0; i < Wheels.Length; i++)
            {
                HardPoints[i].localPosition = new Vector3(Wheels[i].localPosition.x, 0, Wheels[i].localPosition.z);

                wheelSkids[i] = Instantiate(WheelSkid, Wheels[i].GetChild(0)).GetComponent<WheelSkid>();
                setWheelSkidvalues_Start(i, SkidMarkController_Self.GetComponent<Skidmarks>(), wheelRadius);
            }
            MaxSpringDistance = Mathf.Abs(Wheels[0].localPosition.y - HardPoints[0].localPosition.y) + 0.1f + wheelRadius;

            wheelBase = Vector3.Distance(Wheels[0].position, Wheels[2].position);
            rearTrack = Vector3.Distance(Wheels[0].position, Wheels[1].position);
        }

        private void Start()
        {
            CentreOfMass_ground = (HardPoints[0].localPosition + HardPoints[1].localPosition + HardPoints[2].localPosition + HardPoints[3].localPosition) / 4;
            rb.centerOfMass = CentreOfMass_ground;

            ChangeOffset();
        }

        private void ChangeOffset()
        => _bodyMesh.transform.position = PositonOffset;

        public void ChangeForces(float acceleration, float rotaion, float brakeInput)
        {
            AccelerationInput = acceleration;
            steerInput = rotaion;
            _brakeInput = brakeInput;

            localVehicleVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            AckermannSteering(steerInput);

            float suspensionForce = 0;
            for (int i = 0; i < Wheels.Length; i++)
            {
                bool wheelIsGrounded = TryGetHitGround(MaxSpringDistance, HardPoints[i].position,out wheelHits[i]);

                AddSuspensionForce(HardPoints[i].position, Wheels[i], wheelHits[i], wheelIsGrounded, out suspensionForce, i);

                GroundedCheckPerWheel(wheelIsGrounded);

                tireVisual(wheelIsGrounded, Wheels[i], HardPoints[i], wheelHits[i].distance, i);
                setWheelSkidvalues_Update(i, skidTotal[i], wheelHits[i].point, wheelHits[i].normal);

            }

            vehicleIsGrounded = (NumberOfGroundedWheels > 1);

            if (vehicleIsGrounded)
            {
                AddAcceleration(AccelerationInput);
                AddRollingResistance();
                brakeLogic(_brakeInput);
                bodyAnimation();

                //AutoBalence
                if (rb.centerOfMass != CentreOfMass_ground)
                {
                    rb.centerOfMass = CentreOfMass_ground;
                }

                // angular drag
                rb.angularDamping = 1;

                //downforce
                rb.AddForce(-transform.up * DownForce * rb.mass);
            }
            else
            {
                if (rb.centerOfMass != CenterOfMass_air.localPosition)
                {
                    rb.centerOfMass = CenterOfMass_air.localPosition;
                }

                // angular drag
                rb.angularDamping = 0.1f;
            }

            //friction
            for (int i = 0; i < Wheels.Length; i++)
            {
                if (i < 2)
                {
                    AddLateralFriction(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1, suspensionForce, i);
                }
                else
                {
                    if (_brakeInput > 0.1f)
                    {
                        AddLateralFriction(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, driftFactor, suspensionForce, i);
                    }
                    else
                    {
                        AddLateralFriction(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1, suspensionForce, i);
                    }

                }
            }


            NumberOfGroundedWheels = 0; //reset grounded int


            //grounded property for event
            if (GroundedProperty != vehicleIsGrounded)
            {
                GroundedProperty = vehicleIsGrounded;
            }

        }

        void AddAcceleration(float accelerationInput)
        {
            float deltaSpeed = Acceleration * accelerationInput * Time.fixedDeltaTime;
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed) * AccelerationCurve.Evaluate(Mathf.Abs(localVehicleVelocity.z / MaxSpeed));

            if (accelerationInput > 0 && localVehicleVelocity.z < 0 || accelerationInput < 0 && localVehicleVelocity.z > 0)
            {
                deltaSpeed = (1 + Mathf.Abs(localVehicleVelocity.z / MaxSpeed)) * Acceleration * accelerationInput * Time.fixedDeltaTime;
            }
            if (_brakeInput < 0.1f && localVehicleVelocity.z < MaxSpeed)
            {
                rb.linearVelocity += transform.forward * deltaSpeed;
            }

        }

        void AddRollingResistance()
        {
            float localSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            float deltaSpeed = RollingResistance * Time.fixedDeltaTime * Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if (AccelerationInput == 0)
            {
                if (localSpeed > 0)
                {
                    rb.linearVelocity -= transform.forward * deltaSpeed;
                }
                else
                {
                    rb.linearVelocity += transform.forward * deltaSpeed;
                }
            }

        }

        void brakeLogic(float brakeInput)
        {
            float localSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            float deltaSpeed = brakeAcceleration * brakeInput * Time.fixedDeltaTime * Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if (localSpeed > 0)
            {
                rb.linearVelocity -= transform.forward * deltaSpeed;
            }
            else
            {
                rb.linearVelocity += transform.forward * deltaSpeed;
            }

        }

        void AddSuspensionForce(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool WheelIsGrounded, out float SuspensionForce, int WheelNum)
        {
            if (WheelIsGrounded)
            {
                Vector3 springDir = wheelHit.normal;
                float offset = (MaxSpringDistance + 0.1f - wheelHit.distance) / (MaxSpringDistance - wheelRadius - 0.1f);

                float vel = -((offset - offset_Prev[WheelNum]) / Time.fixedDeltaTime);

                Vector3 wheelWorldVel = rb.GetPointVelocity(wheelHit.point);
                float WheelVel = Vector3.Dot(springDir, wheelWorldVel);

                offset_Prev[WheelNum] = offset;
                if (offset < 0.3f)
                {
                    vel = 0;
                }
                else if (vel < 0 && offset > 0.6f && WheelVel < 10)
                {
                    vel *= 10;
                }

                float TotalSpringForce = offset * offset * springForce;
                float totalDampingForce = Mathf.Clamp(-(vel * springDamper), -0.25f * rb.mass * Mathf.Abs(WheelVel) / Time.fixedDeltaTime, 0.25f * rb.mass * Mathf.Abs(WheelVel) / Time.fixedDeltaTime);
                if ((MaxSpringDistance + 0.1f - wheelHit.distance) < 0.1f)
                {
                    totalDampingForce = 0;
                }
                float force = TotalSpringForce + totalDampingForce;
                SuspensionForce = force;

                rb.AddForceAtPosition(springDir * force, hardPoint);

            }
            else
            {
                SuspensionForce = 0;
            }

        }
        
        public void AddLateralFriction(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool wheelIsGrounded, float factor, float suspensionForce, int wheelNum)
        {
            if (wheelIsGrounded)
            {
                Vector3 SurfaceNormal = wheelHit.normal;

                Vector3 sideVelocity = (wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).x) * wheel.right;
                Vector3 forwardVelocity = (wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).z) * wheel.forward;

                slipCoeff[wheelNum] = sideVelocity.magnitude / (sideVelocity.magnitude + Mathf.Clamp(forwardVelocity.magnitude, 0.1f, forwardVelocity.magnitude));

                Vector3 contactDesiredAccel = -Vector3.ProjectOnPlane(sideVelocity, SurfaceNormal) / Time.fixedDeltaTime;

                Vector3 frictionForce = Vector3.ClampMagnitude(rb.mass * contactDesiredAccel * sideFrictionCurve.Evaluate(slipCoeff[wheelNum]), suspensionForce * FrictionCoefficient);
                frictionForce = suspensionForce * FrictionCoefficient * -sideVelocity.normalized * sideFrictionCurve.Evaluate(slipCoeff[wheelNum]);

                float clampedFrictionForce = Mathf.Min(rb.mass / 4 * contactDesiredAccel.magnitude, -Physics.gravity.y * rb.mass);

                frictionForce = Vector3.ClampMagnitude(frictionForce * forwardFrictionCurve.Evaluate(forwardVelocity.magnitude / MaxSpeed), clampedFrictionForce);
                rb.AddForceAtPosition(frictionForce * factor, hardPoint);
            }

        }

        void AckermannSteering(float steerInput)
        {
            float turnRadius = wheelBase / Mathf.Tan(MaxTurnAngle / Mathf.Rad2Deg);
            if (steerInput > 0) //is turning right
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            }
            else if (steerInput < 0) //is turning left
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            }
            else
            {
                ackermennLeftAngle = 0;
                ackermennRightAngle = 0;
            }

            // auto counter steering
            if (localVehicleVelocity.z > 0 && AutoCounterSteer && Mathf.Abs(localVehicleVelocity.x) > 1f)
            {
                ackermennLeftAngle += Vector3.SignedAngle(transform.forward, rb.linearVelocity + transform.forward, transform.up);
                ackermennLeftAngle = Mathf.Clamp(ackermennLeftAngle, -70, 70);
                ackermennRightAngle += Vector3.SignedAngle(transform.forward, rb.linearVelocity + transform.forward, transform.up);
                ackermennRightAngle = Mathf.Clamp(ackermennRightAngle, -70, 70);
            }

            Wheels[0].localRotation = Quaternion.Euler(0, ackermennLeftAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
            Wheels[1].localRotation = Quaternion.Euler(0, ackermennRightAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
        }

        void tireVisual(bool WheelIsGrounded, Transform wheel, Transform hardPoint, float hitDistance, int tireNum)
        {
            if (WheelIsGrounded)
            {
                if (offset_Prev[tireNum] > 0.3f)
                {
                    wheel.localPosition = hardPoint.localPosition + (Vector3.up * wheelRadius) - Vector3.up * (hitDistance);
                }
                else
                {
                    wheel.localPosition = Vector3.Lerp(new Vector3(hardPoint.localPosition.x, wheel.localPosition.y, hardPoint.localPosition.z), hardPoint.localPosition + (Vector3.up * wheelRadius) - Vector3.up * (hitDistance), 0.1f);
                }

            }
            else
            {
                wheel.localPosition = Vector3.Lerp(new Vector3(hardPoint.localPosition.x, wheel.localPosition.y, hardPoint.localPosition.z), hardPoint.localPosition + (Vector3.up * wheelRadius) - Vector3.up * MaxSpringDistance, 0.05f);
            }

            Vector3 wheelVelocity = rb.GetPointVelocity(hardPoint.position);
            float minRotation = (Vector3.Dot(wheelVelocity, wheel.forward) / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
            float maxRotation = (Mathf.Sign(Vector3.Dot(wheelVelocity, wheel.forward)) * MaxSpeed / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
            float wheelRotation = 0;

            if (_brakeInput > 0.1f)
            {
                wheelRotation = 0;
            }
            else if (Mathf.Abs(AccelerationInput) > 0.1f)
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, maxRotation / 2);
                wheelRotation = maxRotation;
            }
            else
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, minRotation);
                wheelRotation = minRotation;
            }
            wheel.GetChild(0).localPosition = Vector3.zero;
            var rot = wheel.GetChild(0).localRotation;
            rot.y = 0;
            rot.z = 0;
            wheel.GetChild(0).localRotation = rot;

            //wheel slip calculation
            forwardSlip[tireNum] = Mathf.Abs(Mathf.Clamp((wheelRotation - minRotation) / (maxRotation), -1, 1));
            if (WheelIsGrounded)
            {
                skidTotal[tireNum] = Mathf.MoveTowards(skidTotal[tireNum], (forwardSlip[tireNum] + slipCoeff[tireNum]) / 2, 0.05f);
            }
            else
            {
                skidTotal[tireNum] = 0;
            }


        }

        void setWheelSkidvalues_Start(int wheelNum, Skidmarks skidmarks, float radius)
        {
            wheelSkids[wheelNum].skidmarks = skidmarks;
            wheelSkids[wheelNum].radius = wheelRadius;
        }
        
        void setWheelSkidvalues_Update(int wheelNum, float skidTotal, Vector3 skidPoint, Vector3 normal)
        {
            wheelSkids[wheelNum].skidTotal = skidTotal;
            wheelSkids[wheelNum].skidPoint = skidPoint;
            wheelSkids[wheelNum].normal = normal;
        }


        void bodyAnimation()
        {
            Vector3 accel = Vector3.ProjectOnPlane((rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime, transform.up);
            accel = transform.InverseTransformDirection(accel);
            lastVelocity = rb.linearVelocity;

            VehicleBody.localRotation = Quaternion.Lerp(VehicleBody.localRotation, Quaternion.Euler(Mathf.Clamp(-accel.z / 10, -forwardBodyTilt, forwardBodyTilt), 0, Mathf.Clamp(accel.x / 5, -sidewaysBodyTilt, sidewaysBodyTilt)), 0.1f);
        }

        void GroundedCheckPerWheel(bool wheelIsGrounded)
        {
            if (wheelIsGrounded)
            {
                NumberOfGroundedWheels += 1;
            }

        }

        #region Vehicle Events Stuff

        public bool GroundedProperty
        {
            get
            {
                return tempGroundedProperty;
            }

            set
            {
                if (value != tempGroundedProperty)
                {
                    tempGroundedProperty = value;
                    if (tempGroundedProperty)
                    {
                        Debug.Log("Grounded");
                        VehicleEvents.OnGrounded?.Invoke();
                    }
                    else
                    {
                        Debug.Log("Take off");
                        VehicleEvents.OnTakeOff?.Invoke();
                    }
                }


            }
        }
        #endregion


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < Wheels.Length; i++)
            {
                Gizmos.DrawLine(HardPoints[i].position + (transform.up * wheelRadius), Wheels[i].position);
                Gizmos.DrawWireSphere(Wheels[i].position, wheelRadius);
                Gizmos.DrawSphere(HardPoints[i].position + (transform.up * wheelRadius), 0.05f);
            }

        }

        private bool TryGetHitGround(float MaxSpringDistance,Vector3 hardPoint,out RaycastHit hit)
        {
            var direction = -transform.up;

            if (Physics.SphereCast(hardPoint + (transform.up * wheelRadius), wheelRadius, direction, out hit, MaxSpringDistance))
            {
                if (IsCollidingWithIgnoreLayer(hit))
                {
                    RaycastHit[] hits = Physics.SphereCastAll(hardPoint + (transform.up * wheelRadius), wheelRadius, direction, MaxSpringDistance);

                    for (int i = 0; i <= hits.Length; i++)
                    {
                        if (IsCollidingWithIgnoreLayer(hits[i]) == false)
                        {
                            hit = hits[i];
                            return true;
                        }
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        private bool IsCollidingWithIgnoreLayer(RaycastHit hit)
        => hit.collider.gameObject.layer == Config.LAYUER_CAR_IGNOR;
    }
}
