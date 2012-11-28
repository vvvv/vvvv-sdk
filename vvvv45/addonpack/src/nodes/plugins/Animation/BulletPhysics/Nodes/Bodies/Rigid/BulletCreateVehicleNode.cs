using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Bullet;
using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;
using VVVV.Bullet.Utils;

namespace VVVV.Bullet.Nodes.Bodies.Rigid
{
    [PluginInfo(Name = "CreateVehicle", Category = "Bullet", Author = "vux",
		Help = "Creates a vehicle", AutoEvaluate = true)]
    public class BulletCreateVehicleNode : AbstractRigidBodyCreator
    {
        int rightIndex = 0;
        int upIndex = 1;
        int forwardIndex = 2;
        float CUBE_HALF_EXTENTS = 1;
        Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);
        Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

        float gEngineForce = 2000.0f;
        float gBreakingForce = 0.0f;

        float maxEngineForce = 2000.0f;//this should be engine/velocity dependent
        float maxBreakingForce = 100.0f;

        float gVehicleSteering = 0.0f;
        float steeringIncrement = 1.0f;
        float steeringClamp = 0.3f;
        public float wheelRadius = 0.7f;
        public float wheelWidth = 0.4f;
        float wheelFriction = 1000;//BT_LARGE_FLOAT;
        float suspensionStiffness = 20.0f;
        float suspensionDamping = 2.3f;
        float suspensionCompression = 4.4f;
        float rollInfluence = 0.1f;//1.0f;

        float suspensionRestLength = 0.6f;


        [Output("Vehicle")]
        ISpread<RaycastVehicle> FOutVehicle;

        public override void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.CanCreate(i))
                {

                    RaycastVehicle vehicle;
                    
                    AbstractRigidShapeDefinition shapedef = this.FShapes[i];
                    ShapeCustomData sc = new ShapeCustomData();
                    sc.ShapeDef = shapedef;


                    CompoundShape compound = new CompoundShape();

                    //List<AbstractRigidShapeDefinition> children = new List<AbstractRigidShapeDefinition>();


                    CollisionShape chassisShape = shapedef.GetShape(sc);
                    Matrix localTrans = Matrix.Translation(Vector3.UnitY);
                    compound.AddChildShape(localTrans, chassisShape);

                    float mass = shapedef.Mass;

                    bool isDynamic = (mass != 0.0f);

                    Vector3 localInertia = Vector3.Zero;
                    if (isDynamic)
                        chassisShape.CalculateLocalInertia(mass, out localInertia);

                    Vector3D pos = this.FPosition[i];
                    Vector4D rot = this.FRotation[i];

                    DefaultMotionState ms = BulletUtils.CreateMotionState(pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);


                    RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, ms, compound, localInertia);
                    RigidBody carChassis = new RigidBody(rbInfo);

                    BodyCustomData bd = new BodyCustomData();

                    carChassis.UserObject = bd;
                    bd.Id = this.FWorld[0].GetNewBodyId();
                    bd.Custom = this.FCustom[i];

                    this.FWorld[0].Register(carChassis);


                    RaycastVehicle.VehicleTuning tuning = new RaycastVehicle.VehicleTuning();
                    VehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(this.FWorld[0].World);
                    vehicle = new RaycastVehicle(tuning, carChassis, vehicleRayCaster);

                    carChassis.ActivationState = ActivationState.DisableDeactivation;
                    this.FWorld[0].World.AddAction(vehicle);

                    float connectionHeight = 1.2f;
                    bool isFrontWheel = true;

                    // choose coordinate system
                    vehicle.SetCoordinateSystem(rightIndex, upIndex, forwardIndex);

                    Vector3 connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * wheelWidth), connectionHeight, 2 * CUBE_HALF_EXTENTS - wheelRadius);
                    WheelInfo a = vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), connectionHeight, 2 * CUBE_HALF_EXTENTS - wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, isFrontWheel);

                    isFrontWheel = false;
                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), connectionHeight, -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * wheelWidth), connectionHeight, -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, isFrontWheel);


                    for (i = 0; i < vehicle.NumWheels; i++)
                    {
                        WheelInfo wheel = vehicle.GetWheelInfo(i);
                        wheel.SuspensionStiffness = suspensionStiffness;
                        wheel.WheelDampingRelaxation = suspensionDamping;
                        wheel.WheelDampingCompression = suspensionCompression;
                        wheel.FrictionSlip = wheelFriction;
                        wheel.RollInfluence = rollInfluence;
                    }

                    FOutVehicle.SliceCount = 1;
                    FOutVehicle[0] = vehicle;
                }
            }
        }
    }
}
