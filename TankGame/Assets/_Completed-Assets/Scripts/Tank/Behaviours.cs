using UnityEngine;
using NPBehave;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {



        private Root CreateBehaviourTree()
        {

            switch (m_Behaviour)
            {

                case 0:
                    return fun();
                case 1:
                    return deadly();
                case 2:
                    return frightened();
                case 3:
                    return unpredictable();
                default:
                    return new Root(new Action(() => Turn(0.1f)));
            }
        }

        /* Actions */
        private void UpdatePerception()
        {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;

            /*
           NavMeshAgent agent;
            agent = GetComponent<NavMeshAgent>();
            agent.SetDestination(TargetTransform().position);
            */


            //environment
            //Vector3 block = transform.TransformDirection(Vector3.forward);
            //bool environment = Physics.Raycast(transform.position, block, 10);




            blackboard["targetDistance"] = localPos.magnitude;
            //blackboard["targetInFront"] = heading.z;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
            blackboard["environment"] = environment();

            blackboard["targetOpen"] = targetOpen();

        }


        //checks to see if the player(or enemy to the enemy tank) is out in the open and not hidden behind buildings
        private bool targetOpen()
        {
            Vector3 targetPosition = new Vector3(TargetTransform().position.x, TargetTransform().position.y + 0.5f, TargetTransform().position.z + 0.88f);
            RaycastHit hit;
           // Debug.DrawRay(currentPosition, targetPosition - currentPosition, Color.cyan);
            if(Physics.SphereCast(transform.position, 0.5f,  targetPosition - transform.position, out hit))
            {
                //the spherecast will hit anything with a collider but will return true if it hits anything with the name tank.
                if (hit.collider.gameObject.name.Contains("Tank"))
                {
                    return true;
                }
            }
            return false;
        }
   
        
        //checks to see if there is any part of the environment is in front of the tank.
        bool environment()
        {
            Vector3 block = transform.TransformDirection(Vector3.forward);
            if(Physics.Raycast(transform.position, block, 5))
                {
                return true;
                }

            return false;
        }

            
        /*
        public Node Patrol()
        {
            return new Action(() =>  ));


        }

            */


        //TURNING!!
        private Node StopTurning()
        {
            return new Action(() => Turn(0));
        }

        //MOVE!!

        private Node StopMove()
        {
            return new Action(() => Move(0));
        }

        //FIRE!!!
        private Node RandomFire()
        {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

        private Node WeakFire()
        {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 0.5f)));
        }



        private Root fun()
        {

            // add the AI dodging player shells
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(

                        // when the target is in front of the AI, the Ai stops moving and turning and shoots
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(
                                        StopTurning(),
                                        StopMove(),
                                        new Action(() => Fire(0.5f)))
                                        ),


                        new BlackboardCondition("targetDistance",
                                                    Operator.IS_GREATER_OR_EQUAL, 10f,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Selector(
                            // if the AI reaches a building or part of the envvironment it stops then turns right and then proceeds forward
                        new BlackboardCondition("environment",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,

                                                new Sequence(
                                                new Action(() => Turn(0.7f)),
                                                new Action(() => Move(0.7f))
                                                         )
                                                      )
                                                 )
                                                ),

                        

                        new BlackboardCondition("targetOpen",
                        Operator.IS_EQUAL, true,
                        Stops.IMMEDIATE_RESTART,
                        //it runs the children sequentially until it succeeds, but also runs it til it fails, if it succeeds to turn right, dont turn left, if it fails to turn right then turn right
                        new Sequence(
                                    new Selector(
                                 //if the target is in fron =t of the AI, it moves forward
                                                new BlackboardCondition("targetInFront",
                                                            Operator.IS_EQUAL, true,
                                                            Stops.IMMEDIATE_RESTART,
                                                            new Action(() => Move(0.5f))),



                                    // of the target is on the right of the AI, it turns right
                                                new BlackboardCondition("targetOnRight",
                                                                        Operator.IS_EQUAL, true,
                                                                     Stops.IMMEDIATE_RESTART,
                                                       
                                    // Turn right toward target
                                               new Action(() => Turn(0.7f))),

                                    // of the target is on the right of the AI, it turns right
                                                new BlackboardCondition("targetOnRight",
                                                                        Operator.IS_EQUAL, false,
                                                                     Stops.IMMEDIATE_RESTART,
                                               // Turn right toward target
                                               new Action(() => Turn(-0.7f))),

                                                
                                                StopMove()

                                         )
                                    )
                                )
                                
            


                             )                        
                        )
                   );


        }






        private Root deadly()
        {

            /*
        // Constantly spin and fire on the spot 
        private Root deadly(float turn, float shoot)
        {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));

        }
        */

            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        // when the target is in front of the AI, the Ai stops moving and turning and shoots
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,

                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        StopMove(),
                                        new Wait(0.2f),
                                        WeakFire())),

                            // if the AI reaches a building or part of the envvironment it stops then turns right and then proceeds forward
                            new BlackboardCondition("environment",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                        StopMove(),
                                                        new Action(() => Turn(0.7f)),
                                                        new Action(() => Move(0.5f)))),


                        //if the target is in fron =t of the AI, it moves forward
                        new BlackboardCondition("targetInFront",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,

                            new Action(() => Move(0.5f))),

                        // of the target is on the right of the AI, it turns right
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,


                            // Turn right toward target
                            new Action(() => Turn(0.7f))),
                            // Turn left toward target

                            new Action(() => Turn(-0.7f))



                    )
                )
            );

        }




        // Turn to face your opponent and fire
        private Root frightened()
        {

            //add better movement, and better shooting control

            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(



                        // when the target is in front of the AI, the Ai stops moving and turning and shoots
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,


                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        StopMove(),
                                        new Wait(0.2f),
                                        WeakFire())),



                            // if the AI reaches a building or part of the envvironment it stops then turns right and then proceeds forward

                            new BlackboardCondition("environment",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,

                                                    new Sequence(
                                                        StopMove(),
                                                        new Action(() => Turn(0.7f)),
                                                        new Action(() => Move(0.5f)))),





                        //if the target is in fron =t of the AI, it moves forward
                        new BlackboardCondition("targetInFront",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,

                            new Action(() => Move(0.5f))),





                        // of the target is on the right of the AI, it turns right
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,


                            // Turn right toward target
                            new Action(() => Turn(0.7f))),
                            // Turn left toward target

                            new Action(() => Turn(-0.7f))



                    )
                )
            );

        }


        /*


        private Root fun()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,


                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        StopMove(),
                                        new Wait(1f),
                                        WeakFire())),

                        new BlackboardCondition("targetDistance",
                                                    Operator.IS_GREATER_OR_EQUAL, 20f,
                                                    Stops.IMMEDIATE_RESTART,

                            new Action(() => Move(0.2f))),



                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,


                            // Turn right toward target
                            new Action(() => Turn(0.5f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.5f))

                    )
                )
            );

        }


    */



        private Root unpredictable()
        {

            // add the AI dodging player shells

            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(



                        // when the target is in front of the AI, the Ai stops moving and turning and shoots
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,


                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        StopMove(),
                                        new Wait(0.2f),
                                        WeakFire())),



                            // if the AI reaches a building or part of the envvironment it stops then turns right and then proceeds forward

                            new BlackboardCondition("environment",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,

                                                    new Sequence(
                                                        StopMove(),
                                                        new Action(() => Turn(0.7f)),
                                                        new Action(() => Move(0.5f)))),





                        //if the target is in fron =t of the AI, it moves forward
                        new BlackboardCondition("targetInFront",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,

                            new Action(() => Move(0.5f))),





                        // of the target is on the right of the AI, it turns right
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,


                            // Turn right toward target
                            new Action(() => Turn(0.7f))),
                            // Turn left toward target

                            new Action(() => Turn(-0.7f))



                    )
                )
            );

        }

    }
}