using UnityEngine;
using System.Collections;

namespace Pathfinding {
    using Pathfinding.RVO;
    using Pathfinding.Util;
    using System.Collections.Generic;

    /** Base class for AIPath and RichAI.
	 * This class holds various methods and fields that are common to both AIPath and RichAI.
	 *
	 * \see #Pathfinding.AIPath
	 * \see #Pathfinding.RichAI
	 * \see #Pathfinding.IAstarAI (all movement scripts implement this interface)
	 */
    [RequireComponent(typeof(Seeker))]
	public abstract class AIBase : VersionedMonoBehaviour {
		/** Determines how often the agent will search for new paths (in seconds).
		 * The agent will plan a new path to the target every N seconds.
		 *
		 * If you have fast moving targets or AIs, you might want to set it to a lower value.
		 *
		 * \see #RepeatTrySearchPath
		 */
		public float repathRate = 0.5f;

		/** \copydoc Pathfinding::IAstarAI::canSearch */
		[UnityEngine.Serialization.FormerlySerializedAs("repeatedlySearchPaths")]
		public bool canSearch = true;

		/** \copydoc Pathfinding::IAstarAI::canMove */
		public bool canMove = true;

		/** Max speed in world units per second */
		[UnityEngine.Serialization.FormerlySerializedAs("speed")]
		public float maxSpeed = 1;

		/** Gravity to use.
		 * If set to (NaN,NaN,NaN) then Physics.Gravity (configured in the Unity project settings) will be used.
		 * If set to (0,0,0) then no gravity will be used and no raycast to check for ground penetration will be performed.
		 */
		public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

		/** Layer mask to use for ground placement.
		 * Make sure this does not include the layer of any colliders attached to this gameobject.
		 *
		 * \see #gravity
		 * \see https://docs.unity3d.com/Manual/Layers.html
		 */
		public LayerMask groundMask = -1;

		/** Offset along the Y coordinate for the ground raycast start position.
		 * Normally the pivot of the character is at the character's feet, but you usually want to fire the raycast
		 * from the character's center, so this value should be half of the character's height.
		 *
		 * A green gizmo line will be drawn upwards from the pivot point of the character to indicate where the raycast will start.
		 *
		 * \see #gravity
		 */
		public float centerOffset = 1;

		/** If true, the forward axis of the character will be along the Y axis instead of the Z axis.
		 *
		 * For 3D games you most likely want to leave this the default value which is false.
		 * For 2D games you most likely want to change this to true as in 2D games you usually
		 * want the Y axis to be the forwards direction of the character.
		 *
		 * \shadowimage{aibase_forward_axis.png}
		 */
		public bool rotationIn2D = false;

		/** Position of the agent.
		 * If #updatePosition is true then this value will be synchronized every frame with Transform.position.
		 */
		protected Vector3 simulatedPosition;

		/** Rotation of the agent.
		 * If #updateRotation is true then this value will be synchronized every frame with Transform.rotation.
		 */
		protected Quaternion simulatedRotation;

		/** Position of the agent.
		 * In world space.
		 * If #updatePosition is true then this value is idential to transform.position.
		 * \see #Teleport
		 * \see #Move
		 */
		public Vector3 position { get { /*Debug.Log(updatePosition + " in GetFeetPosition position"); */ return updatePosition ? tr.position : simulatedPosition; } }

		/** Rotation of the agent.
		 * If #updateRotation is true then this value is identical to transform.rotation.
		 */
		public Quaternion rotation { get { return updateRotation ? tr.rotation : simulatedRotation; } }

		/** Accumulated movement deltas from the #Move method */
		Vector3 accumulatedMovementDelta = Vector3.zero;

		/** Current desired velocity of the agent (does not include local avoidance and physics).
		 * Lies in the movement plane.
		 */
		public Vector2 velocity2D; //J8045: made public in order for other scripts to obtain the movement direction

		/** Velocity due to gravity.
		 * Perpendicular to the movement plane.
		 *
		 * When the agent is grounded this may not accurately reflect the velocity of the agent.
		 * It may be non-zero even though the agent is not moving.
		 */
		protected float verticalVelocity;

		/** Cached Seeker component */
		protected Seeker seeker;

		/** Cached Transform component */
		protected Transform tr;

		/** Cached Rigidbody component */
		protected Rigidbody rigid;

		/** Cached Rigidbody component */
		protected Rigidbody2D rigid2D;

		/** Cached CharacterController component */
		protected CharacterController controller;


		/** Plane which this agent is moving in.
		 * This is used to convert between world space and a movement plane to make it possible to use this script in
		 * both 2D games and 3D games.
		 */
		public IMovementPlane movementPlane = GraphTransform.identityTransform;

		/** Determines if the character's position should be coupled to the Transform's position.
		 * If false then all movement calculations will happen as usual, but the object that this component is attached to will not move
		 * instead only the #position property will change.
		 *
		 * This is useful if you want to control the movement of the character using some other means such
		 * as for example root motion but still want the AI to move freely.
		 * \see Combined with calling #MovementUpdate from a separate script instead of it being called automatically one can take a similar approach to what is documented here: https://docs.unity3d.com/Manual/nav-CouplingAnimationAndNavigation.html
		 *
		 * \see #canMove which in contrast to this field will disable all movement calculations.
		 * \see #updateRotation
		 */
		[System.NonSerialized]
		public bool updatePosition = true;

		/** Determines if the character's rotation should be coupled to the Transform's rotation.
		 * If false then all movement calculations will happen as usual, but the object that this component is attached to will not rotate
		 * instead only the #rotation property will change.
		 *
		 * \see #updatePosition
		 */
		[System.NonSerialized]
		public bool updateRotation = true;

		/** Indicates if gravity is used during this frame */
		protected bool usingGravity { get; private set; }

		/** Delta time used for movement during the last frame */
		protected float lastDeltaTime;

		/** Last frame index when #prevPosition1 was updated */
		protected int prevFrame;

		/** Position of the character at the end of the last frame */
		protected Vector3 prevPosition1;

		/** Position of the character at the end of the frame before the last frame */
		protected Vector3 prevPosition2;

		/** Amount which the character wants or tried to move with during the last frame */
		protected Vector2 lastDeltaPosition;

		/** Only when the previous path has been calculated should the script consider searching for a new path */
		protected bool waitingForPathCalculation = false;

		/** Time when the last path request was started */
		protected float lastRepath = float.NegativeInfinity;

		[UnityEngine.Serialization.FormerlySerializedAs("target")][SerializeField][HideInInspector]
		Transform targetCompatibility;

		/** True if the Start method has been executed.
		 * Used to test if coroutines should be started in OnEnable to prevent calculating paths
		 * in the awake stage (or rather before start on frame 0).
		 */
		bool startHasRun = false;

		/** Target to move towards.
		 * The AI will try to follow/move towards this target.
		 * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
		 *
		 * \deprecated In 4.1 this will automatically add a \link Pathfinding.AIDestinationSetter AIDestinationSetter\endlink component and set the target on that component.
		 * Try instead to use the #destination property which does not require a transform to be created as the target or use
		 * the AIDestinationSetter component directly.
		 */
		[System.Obsolete("Use the destination property or the AIDestinationSetter component instead")]
		public Transform target {
			get {
				var setter = GetComponent<AIDestinationSetter>();
				return setter != null ? setter.target : null;
			}
			set {
				targetCompatibility = null;
				var setter = GetComponent<AIDestinationSetter>();
				if (setter == null) setter = gameObject.AddComponent<AIDestinationSetter>();
				setter.target = value;
				destination = value != null ? value.position : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
		}

		/** \copydoc Pathfinding::IAstarAI::destination */
		public Vector3 destination { get; set; }

		/** \copydoc Pathfinding::IAstarAI::velocity */
		public Vector3 velocity {
			get {
				return lastDeltaTime > 0.000001f ? (prevPosition1 - prevPosition2) / lastDeltaTime : Vector3.zero;
			}
		}

		/** Velocity that this agent wants to move with.
		 * Includes gravity and local avoidance if applicable.
		 */
		public Vector3 desiredVelocity { get { return lastDeltaTime > 0.00001f ? movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity) : Vector3.zero; } }

		/** \copydoc Pathfinding::IAstarAI::isStopped */
		public bool isStopped { get; set; }

		/** \copydoc Pathfinding::IAstarAI::onSearchPath */
		public System.Action onSearchPath { get; set; }

		/** True if the path should be automatically recalculated as soon as possible */
		protected virtual bool shouldRecalculatePath {
			get {
				return Time.time - lastRepath >= repathRate && !waitingForPathCalculation && canSearch && !float.IsPositiveInfinity(destination.x);
			}
		}

		protected AIBase () {
			// Note that this needs to be set here in the constructor and not in e.g Awake
			// because it is possible that other code runs and sets the destination property
			// before the Awake method on this script runs.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		protected virtual void FindComponents () {
			tr = transform;
			seeker = GetComponent<Seeker>();
			// Find attached movement components
			controller = GetComponent<CharacterController>();
			rigid = GetComponent<Rigidbody>();
			rigid2D = GetComponent<Rigidbody2D>();
		}

		/** Called when the component is enabled */
		protected virtual void OnEnable () {
			FindComponents();
			// Make sure we receive callbacks when paths are calculated
			seeker.pathCallback += OnPathComplete;
			Init();
		}

		/** Starts searching for paths.
		 * If you override this method you should in most cases call base.Start () at the start of it.
		 * \see #Init
		 */
		protected virtual void Start () {
			startHasRun = true;
			Init();
		}

		void Init () {
			if (startHasRun) {
				// Clamp the agent to the navmesh (which is what the Teleport call will do essentially. Though only some movement scripts require this, like RichAI).
				// The Teleport call will also make sure some variables are properly initialized (like #prevPosition1 and #prevPosition2)
				Teleport(position, false);
				lastRepath = float.NegativeInfinity;
				if (shouldRecalculatePath) SearchPath();
			}
		}

		/** \copydoc Pathfinding::IAstarAI::Teleport */
		public virtual void Teleport (Vector3 newPosition, bool clearPath = true) {
			if (clearPath) CancelCurrentPathRequest();
			prevPosition1 = prevPosition2 = simulatedPosition = newPosition;
			if (updatePosition) tr.position = newPosition;
			if (clearPath) SearchPath();
		}

		protected void CancelCurrentPathRequest () {
			waitingForPathCalculation = false;
			// Abort calculation of the current path
			if (seeker != null) seeker.CancelCurrentPathRequest();
		}

		protected virtual void OnDisable () {
			CancelCurrentPathRequest();

			// Make sure we no longer receive callbacks when paths complete
			seeker.pathCallback -= OnPathComplete;

			velocity2D = Vector3.zero;
			accumulatedMovementDelta = Vector3.zero;
			verticalVelocity = 0f;
			lastDeltaTime = 0;
		}

		/** Called every frame.
		 * If no rigidbodies are used then all movement happens here.
		 */
		protected virtual void Update () {
            //readjust position according to position of sprite in children
            GameObject sprObj = GetComponentInChildren<SpriteRenderer>().gameObject;
            //tr.position += sprObj.transform.localPosition;
            //if(sprObj.transform.localPosition != Vector3.zero)
            //{
            //    Debug.Log("localPosition of sprObj: " + sprObj.transform.localPosition);
            //}
            ////Debug.Log("after adjusting position to be sprObj's position, sprObj's local position: " + sprObj.transform.localPosition);
            //sprObj.transform.localPosition = Vector3.zero;
            tr.position = sprObj.transform.position;
            sprObj.transform.localPosition = Vector3.zero;

            //J: as todo: readjust rotation according to sprite rotation in children
            //Or, just could freeze rotation ...
            //noting of how paths would be not 'quantized' but smooth and\or such, and noting of maybe <<classifying><YKWIM>> angles into different sprites ...
            //tr.rotation


            ////Debug.Log("after adjusting position to be sprObj's position and not adjusting back, sprObj's local position: "+ sprObj.transform.localPosition);

            if (shouldRecalculatePath) SearchPath();

			// If gravity is used depends on a lot of things.
			// For example when a non-kinematic rigidbody is used then the rigidbody will apply the gravity itself
			// Note that the gravity can contain NaN's, which is why the comparison uses !(a==b) instead of just a!=b.
			usingGravity = !(gravity == Vector3.zero) && (!updatePosition || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic)));
			if (rigid == null && rigid2D == null && canMove) {
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/** Called every physics update.
		 * If rigidbodies are used then all movement happens here.
		 */
		protected virtual void FixedUpdate () {
			if (!(rigid == null && rigid2D == null) && canMove) {
                //readjust position according to position of sprite in children
                GameObject sprObj = GetComponentInChildren<SpriteRenderer>().gameObject;
                tr.position += sprObj.transform.localPosition;
                sprObj.transform.position -= sprObj.transform.localPosition;

                Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.fixedDeltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/** \copydoc Pathfinding::IAstarAI::MovementUpdate */
		public void MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			lastDeltaTime = deltaTime;
			MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
		}

		/** Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not */
		protected abstract void MovementUpdateInternal (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

		/** Outputs the start point and end point of the next automatic path request.
		 * This is a separate method to make it easy for subclasses to swap out the endpoints
		 * of path requests. For example the #LocalSpaceRichAI script which requires the endpoints
		 * to be transformed to graph space first.
		 */
		protected virtual void CalculatePathRequestEndpoints (out Vector3 start, out Vector3 end) {
			start = GetFeetPosition();
			end = destination;
		}

		/** \copydoc Pathfinding::IAstarAI::SearchPath */
		public virtual void SearchPath () {
			if (float.IsPositiveInfinity(destination.x)) return;
			if (onSearchPath != null) onSearchPath();

			lastRepath = Time.time;
			waitingForPathCalculation = true;

			seeker.CancelCurrentPathRequest();

			Vector3 start, end;
			CalculatePathRequestEndpoints(out start, out end);

			// Alternative way of requesting the path
			//ABPath p = ABPath.Construct(start, end, null);
			//seeker.StartPath(p);

			// This is where we should search to
			// Request a path to be calculated from our current position to the destination
			seeker.StartPath(start, end);
		}

		/** Position of the base of the character.
		 * This is used for pathfinding as the character's pivot point is sometimes placed
		 * at the center of the character instead of near the feet. In a building with multiple floors
		 * the center of a character may in some scenarios be closer to the navmesh on the floor above
		 * than to the floor below which could cause an incorrect path to be calculated.
		 * To solve this the start point of the requested paths is always at the base of the character.
		 */
		public virtual Vector3 GetFeetPosition () {
			// Use the base of the CharacterController.
			// If updatePosition is false then fall back to only using the simulated position
			if (controller != null && controller.enabled && updatePosition)
            { //J: to adjust position based on RigidBody movements ... in pathfinding ...
                //Debug.Log("GetFeetPosition here");
                return tr.TransformPoint(controller.center) - Vector3.up*controller.height*0.5F;
			}
            //Debug.Log("position: " + position + "; transform position of Ogre: "+tr.Find("Enemy1-Ogre").transform.position+"; RigidBody2D position: " + GetComponentInChildren<Rigidbody2D>().position);
			return position;
		}

		/** Called when a requested path has been calculated */
		protected abstract void OnPathComplete (Path newPath);

		/** \copydoc Pathfinding::IAstarAI::SetPath */
		public void SetPath (Path path) {
			if (path.PipelineState == PathState.Created) {
				// Path has not started calculation yet
				lastRepath = Time.time;
				waitingForPathCalculation = true;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path);
			} else if (path.PipelineState == PathState.Returned) {
				// Path has already been calculated

				// We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
				else throw new System.ArgumentException("If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

				OnPathComplete(path);
			} else {
				// Path calculation has been started, but it is not yet complete. Cannot really handle this.
				throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
			}
		}

		/** Accelerates the agent downwards.
		 * \see #verticalVelocity
		 * \see #gravity
		 */
		protected void ApplyGravity (float deltaTime) {
			// Apply gravity
			if (usingGravity) {
				float verticalGravity;
				velocity2D += movementPlane.ToPlane(deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity : gravity), out verticalGravity);
				verticalVelocity += verticalGravity;
			} else {
				verticalVelocity = 0;
			}
		}

		/** Calculates how far to move during a single frame */
		protected Vector2 CalculateDeltaToMoveThisFrame (Vector2 position, float distanceToEndOfPath, float deltaTime) {
			// Direction and distance to move during this frame
			return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
		}

		/** Simulates rotating the agent towards the specified direction and returns the new rotation.
		 * \param direction Direction in world space to rotate towards.
		 * \param maxDegrees Maximum number of degrees to rotate this frame.
		 *
		 * Note that this only calculates a new rotation, it does not change the actual rotation of the agent.
		 * Useful when you are handling movement externally using #FinalizeMovement but you want to use the built-in rotation code.
		 *
		 * \see #rotationIn2D
		 */
		public Quaternion SimulateRotationTowards (Vector3 direction, float maxDegrees) {
			return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
		}

		/** Simulates rotating the agent towards the specified direction and returns the new rotation.
		 * \param direction Direction in the movement plane to rotate towards.
		 * \param maxDegrees Maximum number of degrees to rotate this frame.
		 *
		 * Note that this only calculates a new rotation, it does not change the actual rotation of the agent.
		 *
		 * \see #rotationIn2D
		 * \see #movementPlane
		 */
		protected Quaternion SimulateRotationTowards (Vector2 direction, float maxDegrees) {
			if (direction != Vector2.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(movementPlane.ToWorld(direction, 0), movementPlane.ToWorld(Vector2.zero, 1));
				// This causes the character to only rotate around the Z axis
				if (rotationIn2D) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.RotateTowards(simulatedRotation, targetRotation, maxDegrees);
			}
			return simulatedRotation;
		}

		/** \copydoc Pathfinding::IAstarAI::Move */
		public virtual void Move (Vector3 deltaPosition) {
			accumulatedMovementDelta += deltaPosition;
		}

		/** Moves the agent to a position.
		 * \param nextPosition New position of the agent.
		 * \param nextRotation New rotation of the agent.
		 *
		 * This is used if you want to override how the agent moves. For example if you are using
		 * root motion with Mecanim.
		 *
		 * This will use a CharacterController, Rigidbody, Rigidbody2D or the Transform component depending on what options
		 * are available.
		 *
		 * The agent will be clamped to the navmesh after the movement (if such information is available, generally this is only done by the RichAI component).
		 *
		 * \see #MovementUpdate for some example code.
		 * \see #controller, #rigid, #rigid2D
		 */
		public virtual void FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			FinalizeRotation(nextRotation);
			FinalizePosition(nextPosition); //J: to roll back if collision occurred
		}

        public bool rotUpdatedFlipVar = false;
        void FinalizeRotation (Quaternion nextRotation) {

            //Quaternion rotation_beforeUpdate = tr.rotation;
            //float rotation2D_beforeUpdate = 0;
            //if (rigid != null) rotation_beforeUpdate = rigid.rotation;
            //else if (rigid2D != null) rotation2D_beforeUpdate = rigid2D.rotation;
            //else rotation_beforeUpdate = tr.rotation;

            simulatedRotation = nextRotation;
            if (updateRotation) {
				if (rigid != null) rigid.MoveRotation(nextRotation);
				else if (rigid2D != null) rigid2D.MoveRotation(nextRotation.eulerAngles.z);
				else tr.rotation = nextRotation;
			}
            
            //J: adding a constraint that would be for preventing collisions in the movement

            //check collisions
            //bool collided = false;
            //rotUpdatedFlipVar = !rotUpdatedFlipVar;
            //foreach (GameObject g in GameObject.FindGameObjectsWithTag("Enemy"))
            //{
            //    bool hasRigid = rigid == null && rigid2D == null;
            //    bool otherHasRigid = g.GetComponent<AIPath>().rigid == null && g.GetComponent<AIPath>().rigid2D == null;
            //    bool checkCollision = false; //bool used to determine whether collision is to be checked with the other gameObject
            //    if (hasRigid == otherHasRigid) //if within the same 'group' to be updated
            //    {
            //        if (rotUpdatedFlipVar == g.GetComponent<AIPath>().rotUpdatedFlipVar) //if the other GameObject already updated
            //        {
            //            //check collision
            //            checkCollision = true;
            //        }
            //    }
            //    else //check collisions with all of those not within the same 'group'
            //    {
            //        checkCollision = true;
            //    }
            //    if (checkCollision && gameObject.GetComponentInChildren<Renderer>().bounds.Intersects(g.GetComponentInChildren<Renderer>().bounds) && gameObject != g/* - noting of whether these would be equivalent or a copy/reference ...being a reference ...*/)
            //    {
            //        //Debug.Log("Intersecting BoxCollider2Ds");
            //        collided = true;
            //        break;
            //    }
            //}

            ////J: rollback on collision
            //if(collided)
            //{
            //    Debug.Log("Collided; rolling back");
            //    simulatedRotation = rotation_beforeUpdate; //J: well, makes the simulated rotation just no rotation on collision
            //    if (updateRotation)
            //    {
            //        if (rigid != null) rigid.MoveRotation(rotation_beforeUpdate);
            //        else if (rigid2D != null) rigid2D.MoveRotation(rotation2D_beforeUpdate);
            //        else tr.rotation = rotation_beforeUpdate;
            //    }
            //}
        }

        //public static List<GameObject> movedGameObjects; //not necessary to have for tracking
        public bool posUpdatedFlipVar = false;

        void FinalizePosition (Vector3 nextPosition) {
            // Use a local variable, it is significantly faster
            Vector3 currentPosition = simulatedPosition;
            //Debug.Log("nextPosition: " + nextPosition + "; currentPosition: " + currentPosition);

            Vector3 currentPosition_beforeUpdate = currentPosition;
			bool positionDirty1 = false;

			if (controller != null && controller.enabled && updatePosition) {
				// Use CharacterController
				// The Transform may not be at #position if it was outside the navmesh and had to be moved to the closest valid position
				tr.position = currentPosition;
				controller.Move((nextPosition - currentPosition) + accumulatedMovementDelta);
				// Grab the position after the movement to be able to take physics into account
				// TODO: Add this into the clampedPosition calculation below to make RVO better respond to physics
				currentPosition = tr.position;
				if (controller.isGrounded) verticalVelocity = 0;
			} else {
				// Use Transform, Rigidbody, Rigidbody2D or nothing at all (if updatePosition = false)
				float lastElevation;
				movementPlane.ToPlane(currentPosition, out lastElevation);
				currentPosition = nextPosition + accumulatedMovementDelta;

				// Position the character on the ground
				if (usingGravity) currentPosition = RaycastPosition(currentPosition, lastElevation);
				positionDirty1 = true;
			}

			// Clamp the position to the navmesh after movement is done
			bool positionDirty2 = false;
			currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

			// Assign the final position to the character if we haven't already set it (mostly for performance, setting the position can be slow)
			if ((positionDirty1 || positionDirty2) && updatePosition) {
				// Note that rigid.MovePosition may or may not move the character immediately.
				// Check the Unity documentation for the special cases.
				if (rigid != null) rigid.MovePosition(currentPosition);
				else if (rigid2D != null) rigid2D.MovePosition(currentPosition);
				else tr.position = currentPosition;
			}

			accumulatedMovementDelta = Vector3.zero;
			simulatedPosition = currentPosition;

            //J: Check GameObject collision with any already-updated GameObjects in the same 'group' to be updated, and all GameObjects in <<what would ><YKWIM>>be <<not in the same 'group' to be updated><YKWIM>>
            //bool collided = false;
            //posUpdatedFlipVar = !posUpdatedFlipVar; //flip the updatedFlipVar as an update to the variable indicating <<update to movement><YKWIM>> having occurred
            //foreach (GameObject g in GameObject.FindGameObjectsWithTag("Enemy"))
            //{
            //    bool hasRigid = rigid == null && rigid2D == null;
            //    bool otherHasRigid = g.GetComponent<AIPath>().rigid == null && g.GetComponent<AIPath>().rigid2D == null;
            //    bool checkCollision = false; //bool used to determine whether collision is to be checked with the other gameObject
            //    if (hasRigid == otherHasRigid) //if within the same 'group' to be updated
            //    {
            //        if (posUpdatedFlipVar == g.GetComponent<AIPath>().posUpdatedFlipVar) //if the other GameObject already updated //actually, allowing collision - ie. not checking collision to rollback - with something that has yet to move could still lead to collision with another of such objects, so could check with all objects ...though could note of deadlock ... unless some of such objects would be able to move ...<noting of this and error in such an algorithm and\or idea with <whatever logic would have been involved> in the past ~1 day, if not 2 days, most immediately prior to 3/20/18<Upd (3/20/18, 6:00PM<< - with any LoC prior, and further detail as unnecessary and N&NIIDWTC or NWFP>< - note this is as I understand it to mean>>):  at 5:xxPM<rmh: , as I would remember><<, as I would remember, if not 4:xxPM to 5:xxPM>< - note this is as I understand it to mean>>, noting of such and what would be covered under (*RegardingReliability#) and this<wl: <<>, with <rmh: w>refactoring such into any of regarding errors and\or such and logic<wl: <, including any of what would be covered under (*EUtilDisc#)>> as unnecessary and N&NIIDWTC or NWFP>< - note this is as I understand it to mean>>>>
            //            //noting of maybe treating agents as obstacles ... though noting of how such would have been done in eg. the built-in pathfinding in Unity - maybe movement being with eg. Rigidbodies and applying force and\or such and\or separation to the point where collision would no longer occur rather than rollback, as noted before ... in allowing movement in eg. converging towards a location and\or such
            //        {
            //            //check collision
            //            checkCollision = true;
            //        }
            //    }
            //    else //check collisions with all of those not within the same 'group'
            //    {
            //        checkCollision = true;
            //    }
            //    if(checkCollision && gameObject != g)
            //    {
            //        //Debug.Log("Bounds of this GameObject: " + gameObject.transform.Find("Enemy1-Ogre").GetComponent<Renderer>().bounds);
            //        //Debug.Log("Bounds of the other GameObject: " + g.transform.Find("Enemy1-Ogre").GetComponent<Renderer>().bounds);
            //        //Debug.Log("Intersects: " + gameObject.GetComponentInChildren<BoxCollider2D>().bounds.Intersects(g.GetComponentInChildren<BoxCollider2D>().bounds));
            //        //Debug.Log("Intersects: " + gameObject.transform.Find("Enemy1-Ogre").GetComponent<Renderer>().bounds.Intersects(g.transform.Find("Enemy1-Ogre").GetComponent<Renderer>().bounds));
            //        //Debug.Log("Intersects: " + gameObject.GetComponentInChildren<Renderer>().bounds.Intersects(g.GetComponentInChildren<Renderer>().bounds));
            //    }
            //    if (checkCollision && gameObject.GetComponentInChildren<Renderer>().bounds.Intersects(g.GetComponentInChildren<Renderer>().bounds) && gameObject != g/* - noting of whether these would be equivalent or a copy/reference ...being a reference ...*/)
            //    {
            //        //Debug.Log("Intersecting BoxCollider2Ds");
            //        collided = true;
            //        break;
            //    }
            //}

            ////J: store GameObject into a global static List of already-updated GameObjects for the current Update/FixedUpdate, which would be used for checking collisions against each one (with rollback on finding an overlap) - noting of whatever cost that there would be in using a global ...
            ///*J: where said List would clear at the end of each Update or FixedUpdate? Depending on what would be the end of all of the moved GameObjects to compare to in checking collision ... well, doing them separately as that's how collisions would be 'accurately' checked for moving 'as much as possible' within the current frame and/or such, each being basically different timepoints on which movement would occur
            //    well, 
            //    checking collisions with all of the objects in a different timepoint, as those objects would be not moving in that timepoint, but just the previously moved objects in the current timepoint (but not the objects that have yet to move, as the collision detection might be different once a movement would be done, and just one collision check between <<each pair of <rmh: Gem>GameObjects><YKWIM>> would be desirable)

            //    with storing all of the existing GameObjects that would be part of FixedUpdate, and 
            //    all of the existing GameObjects that would be part of Update, separately

            //    noting of how Update and FixedUpdate would be separate, with what could be used to determine whether this would be called by Update or FixedUpdate
            //        -well, these conditions: 
            //        rigid == null && rigid2D == null
            //    where maybe all of the GameObjects could be accessed through eg. an enemy tag, and all of the FixedUpdate objects could be processed separately
            //    though if they would be at the same timepoint ...well, there being somewhat double checking ... as possible ... with this being maybe the 'next-best' outcome ... with such and how little that it would eg. lessen the potential of movement due to restrictiveness <<from collision detection><YKWIM>>

            //    or instead of storing such in <a List> \ <a data structure apart from a List>, could check if already moved by maybe having a variable flipping between 1 and 0 or something, and if it has been flipped, then it being considered as having been already updated, and just checking among the GameObjects <in the same group of GameObjects <<in terms of whether they would be updated via Update vs. via FixedUpdate><YKWIM>>> with flipped variables in checking if they updated
            // */


            ////movedGameObjects.Add(gameObject); //not used due to the corresponding data structure being not used

            ////J: End of position updating code

            UpdateVelocity();

            /*J: rollback position and movement ...as well as velocity, ... if collision with an already-updated GameObject would have occurred <wtl: <<once><YKWIM>> >upon the movements that would lead to collision would occur
            J: noting of how to rollback position and movement ... maybe saving the position and velocity and such previously, and then setting the movement stuff again if a collision would have been detected ... and also such of rotation and saving such, maybe all as "prev" values saved somewhere in the movement updating methods and\or such
            well, noting of already-existing "prev" values within this method of Finalize<rmth: Movement>Position
            */
            //if (collided)
            //{
            //    //rollback code
            //    //Debug.Log("collided; currentPosition_beforeUpdate: " + currentPosition_beforeUpdate);

            //    //set nextPosition to position before update
            //    nextPosition = currentPosition_beforeUpdate;

            //    //do the code again if collided - noting of whether this would work as-is in just copying such here
            //    currentPosition = simulatedPosition;
            //    positionDirty1 = false;

            //    if (controller != null && controller.enabled && updatePosition)
            //    {
            //        // Use CharacterController
            //        // The Transform may not be at #position if it was outside the navmesh and had to be moved to the closest valid position
            //        tr.position = currentPosition;
            //        controller.Move((nextPosition - currentPosition) + accumulatedMovementDelta);
            //        // Grab the position after the movement to be able to take physics into account
            //        // TODO: Add this into the clampedPosition calculation below to make RVO better respond to physics
            //        currentPosition = tr.position;
            //        if (controller.isGrounded) verticalVelocity = 0;
            //    }
            //    else
            //    {
            //        // Use Transform, Rigidbody, Rigidbody2D or nothing at all (if updatePosition = false)
            //        float lastElevation;
            //        movementPlane.ToPlane(currentPosition, out lastElevation);
            //        currentPosition = nextPosition + accumulatedMovementDelta;

            //        // Position the character on the ground
            //        if (usingGravity) currentPosition = RaycastPosition(currentPosition, lastElevation);
            //        positionDirty1 = true;
            //    }

            //    // Clamp the position to the navmesh after movement is done
            //    positionDirty2 = false;
            //    currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

            //    // Assign the final position to the character if we haven't already set it (mostly for performance, setting the position can be slow)
            //    if ((positionDirty1 || positionDirty2) && updatePosition)
            //    {
            //        // Note that rigid.MovePosition may or may not move the character immediately.
            //        // Check the Unity documentation for the special cases.
            //        if (rigid != null) rigid.MovePosition(currentPosition);
            //        else if (rigid2D != null) rigid2D.MovePosition(currentPosition);
            //        else tr.position = currentPosition;
            //    }

            //    accumulatedMovementDelta = Vector3.zero;
            //    simulatedPosition = currentPosition;
            //    UpdateVelocity();
            //    //Debug.Log("After the position update, currentPosition: " + currentPosition);
            //}
        }

		protected void UpdateVelocity () {
			var currentFrame = Time.frameCount;

			if (currentFrame != prevFrame) prevPosition2 = prevPosition1;
			prevPosition1 = position;
			prevFrame = currentFrame;
		}

		/** Constrains the character's position to lie on the navmesh.
		 * Not all movement scripts have support for this.
		 *
		 * \param position Current position of the character.
		 * \param positionChanged True if the character's position was modified by this method.
		 *
		 * \returns New position of the character that has been clamped to the navmesh.
		 */
		protected virtual Vector3 ClampToNavmesh (Vector3 position, out bool positionChanged) {
			positionChanged = false;
			return position;
		}

		/** Checks if the character is grounded and prevents ground penetration.
		 * \param position Position of the character in the world.
		 * \param lastElevation Elevation coordinate before the agent was moved. This is along the 'up' axis of the #movementPlane.
		 *
		 * Sets #verticalVelocity to zero if the character is grounded.
		 *
		 * \returns The new position of the character.
		 */
		protected Vector3 RaycastPosition (Vector3 position, float lastElevation) {
			RaycastHit hit;
			float elevation;

			movementPlane.ToPlane(position, out elevation);
			float rayLength = centerOffset + Mathf.Max(0, lastElevation-elevation);
			Vector3 rayOffset = movementPlane.ToWorld(Vector2.zero, rayLength);

			if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, groundMask, QueryTriggerInteraction.Ignore)) {
				// Grounded
				// Make the vertical velocity fall off exponentially. This is reasonable from a physical standpoint as characters
				// are not completely stiff and touching the ground will not immediately negate all velocity downwards. The AI will
				// stop moving completely due to the raycast penetration test but it will still *try* to move downwards. This helps
				// significantly when moving down along slopes as if the vertical velocity would be set to zero when the character
				// was grounded it would lead to a kind of 'bouncing' behavior (try it, it's hard to explain). Ideally this should
				// use a more physically correct formula but this is a good approximation and is much more performant. The constant
				// '5' in the expression below determines how quickly it converges but high values can lead to too much noise.
				verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
				return hit.point;
			}
			return position;
		}

		protected static readonly Color GizmoColorRaycast = new Color(118.0f/255, 206.0f/255, 112.0f/255);

		protected virtual void OnDrawGizmosSelected () {
			// When selected in the Unity inspector it's nice to make the component react instantly if
			// any other components are attached/detached or enabled/disabled.
			// We don't want to do this normally every frame because that would be expensive.
			if (Application.isPlaying) FindComponents();
		}

		protected virtual void OnDrawGizmos () {
			if (!Application.isPlaying || !enabled) FindComponents();

			// Note that the gravity can contain NaN's, which is why the comparison uses !(a==b) instead of just a!=b.
			var usingGravity = !(gravity == Vector3.zero) && (!updatePosition || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic)));
			if (usingGravity && (controller == null || !controller.enabled)) {
				Gizmos.color = GizmoColorRaycast;
				Gizmos.DrawLine(position, position + transform.up*centerOffset);
				Gizmos.DrawLine(position - transform.right*0.1f, position + transform.right*0.1f);
				Gizmos.DrawLine(position - transform.forward*0.1f, position + transform.forward*0.1f);
			}

			if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying) Draw.Gizmos.CircleXZ(destination, 0.2f, Color.blue);
		}

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			#pragma warning disable 618
			if (unityThread && targetCompatibility != null) target = targetCompatibility;
			#pragma warning restore 618
			return 1;
		}
	}
}
