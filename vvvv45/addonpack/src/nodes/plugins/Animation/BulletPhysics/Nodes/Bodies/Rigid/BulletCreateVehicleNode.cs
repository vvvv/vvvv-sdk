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
    [PluginInfo(Name = "CreateVehicle", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Creates a vehicle", AutoEvaluate = true)]
    public class BulletCreateVehicleNode : AbstractRigidBodyCreator
    {
        
    	[Input("Suspension Restlength",DefaultValue = 0.06, IsSingle = true)]
        protected ISpread<float> FsuspensionRestLength;
    	
        [Input("Wheel Friction",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FwheelFriction;
        
        [Input("Suspension Stiffness",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FsuspensionStiffness;
        
        [Input("Damping Relaxation",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FDampingRelaxation;
        
        [Input("Damping Compression",DefaultValue = 4.00, IsSingle = true)]
        protected ISpread<float> FDampingCompression;
        
        [Input("Wheel Radius",DefaultValue = 0.70, IsSingle = true)]
        protected ISpread<float> FwheelRadius;
        
        [Input("Wheel Width",DefaultValue = 0.40, IsSingle = true)]
        protected ISpread<float> FwheelWidth;
        
        [Input("Wheel Distance",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FwheelDistance;
        
        [Input("Roll Influence",DefaultValue = 0.10, IsSingle = true)]
        protected ISpread<float> FrollInfluence;
        
        [Input("Max Suspension Travel",DefaultValue = 500.00, IsSingle = true)]
        protected ISpread<float> FmaxSuspensionTravelCm;
        
        [Input("Max Suspension Force",DefaultValue = 6000.00, IsSingle = true)]
        protected ISpread<float> FmaxSuspensionForce;
        
        [Input("Connection Height",DefaultValue = 1.20, IsSingle = true)]
        protected ISpread<float> FconnectionHeight;
        
       	
    	
    	
    	int rightIndex = 0;
        int upIndex = 1;
        int forwardIndex = 2;
        float CUBE_HALF_EXTENTS = 1;
        Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);
        Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

        float gEngineForce = 2000.0f;
        float gBreakingForce = 0.0f;

        float maxEngineForce = 5.0f;//this should be engine/velocity dependent
        float maxBreakingForce = 100.0f;

        float gVehicleSteering = 0.0f;
        float steeringIncrement = 1.0f;
        float steeringClamp = 0.3f;
        public float wheelRadius = 0.7f;
        public float wheelWidth = 0.4f;
        public bool isFrontWheel = true;

        

        
        [Output("Vehicle")]
        ISpread<RaycastVehicle> FOutVehicle;

        public override void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.CanCreate(i))
                {

                	wheelRadius = FwheelRadius[0];
                	wheelWidth = FwheelWidth[0];
                	CUBE_HALF_EXTENTS = FwheelDistance[0];
                	
                	RaycastVehicle vehicle;
                    
                    AbstractRigidShapeDefinition shapedef = this.FShapes[i];
                    ShapeCustomData sc = new ShapeCustomData();
                    sc.ShapeDef = shapedef;


                    CompoundShape compound = new CompoundShape();

             


                    CollisionShape chassisShape = shapedef.GetShape(sc);
                    Matrix localTrans = Matrix.Translation(Vector3.UnitY);
                    compound.AddChildShape(localTrans, chassisShape);

                    float mass = shapedef.Mass;

                    bool isDynamic = (mass != 0.0f);
                    isFrontWheel = true;

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

                    
                   

                    // choose coordinate system
                    vehicle.SetCoordinateSystem(rightIndex, upIndex, forwardIndex);

                    Vector3 connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * wheelWidth), FconnectionHeight[0], 2 * CUBE_HALF_EXTENTS - wheelRadius);
                    WheelInfo a = vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), FconnectionHeight[0], 2 * CUBE_HALF_EXTENTS - wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel);

                    isFrontWheel = false;
                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), FconnectionHeight[0], -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * wheelWidth), FconnectionHeight[0], -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel);


                    for (i = 0; i < vehicle.NumWheels; i++)
                    {
                        WheelInfo wheel = vehicle.GetWheelInfo(i);
                        wheel.SuspensionStiffness = FsuspensionStiffness[0];
                        wheel.WheelDampingRelaxation = FDampingRelaxation[0];
                        wheel.WheelDampingCompression = FDampingCompression[0];
                        wheel.FrictionSlip = FwheelFriction[0];
                        wheel.RollInfluence = FrollInfluence[0];
                        wheel.MaxSuspensionTravelCm = FmaxSuspensionTravelCm[0];
                        wheel.MaxSuspensionForce = FmaxSuspensionForce[0];
                       
                    }

                    FOutVehicle.SliceCount = 1;
                    FOutVehicle[0] = vehicle;
                }
            }
        }
    }
}
