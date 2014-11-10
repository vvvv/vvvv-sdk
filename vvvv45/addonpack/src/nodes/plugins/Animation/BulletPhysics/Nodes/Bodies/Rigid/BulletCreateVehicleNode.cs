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
        
    	[Input("suspensionRestLength",DefaultValue = 0.06, IsSingle = true)]
        protected ISpread<float> FsuspensionRestLength;
    	
        [Input("wheelFriction",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FwheelFriction;
        
        [Input("suspensionStiffness",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FsuspensionStiffness;
        
        [Input("DampingRelaxation",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FDampingRelaxation;
        
        [Input("DampingCompression",DefaultValue = 4.00, IsSingle = true)]
        protected ISpread<float> FDampingCompression;
        
        [Input("wheelRadius",DefaultValue = 0.70, IsSingle = true)]
        protected ISpread<float> FwheelRadius;
        
        [Input("wheelWidth",DefaultValue = 0.40, IsSingle = true)]
        protected ISpread<float> FwheelWidth;
        
        [Input("wheelDistance",DefaultValue = 1.00, IsSingle = true)]
        protected ISpread<float> FwheelDistance;
        
        [Input("rollInfluence",DefaultValue = 0.10, IsSingle = true)]
        protected ISpread<float> FrollInfluence;
        
        [Input("maxSuspensionTravelCm",DefaultValue = 500.00, IsSingle = true)]
        protected ISpread<float> FmaxSuspensionTravelCm;
        
        [Input("maxSuspensionForce",DefaultValue = 6000.00, IsSingle = true)]
        protected ISpread<float> FmaxSuspensionForce;
        
        [Input("connectionHeight",DefaultValue = 1.20, IsSingle = true)]
        protected ISpread<float> FconnectionHeight;
        
       	[Input("isFrontWheel", IsSingle = true)]
        protected ISpread<bool> isFrontWheel;
    	
    	
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
                    isFrontWheel[0] = true;

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
                    WheelInfo a = vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel[0]);

                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), FconnectionHeight[0], 2 * CUBE_HALF_EXTENTS - wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel[0]);

                    isFrontWheel[0] = false;
                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * wheelWidth), FconnectionHeight[0], -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel[0]);

                    connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * wheelWidth), FconnectionHeight[0], -2 * CUBE_HALF_EXTENTS + wheelRadius);
                    vehicle.AddWheel(connectionPointCS0, wheelDirectionCS0, wheelAxleCS, FsuspensionRestLength[0], wheelRadius, tuning, isFrontWheel[0]);


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
