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

            blackboard["environmentFront"] = environmentFront();
            blackboard["envronmentLeft"] = environmentLeft();
            blackboard["envronmentLeft"] = environmentRight();

            blackboard["targetOpen"] = targetOpen();

        }


        //checks to see if the player(or enemy to the enemy tank) is out in the open and not hidden behind buildings as well as in front of the enemy
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
        bool environmentFront()
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            if(Physics.Raycast(transform.position, forward, 10))
                {
                return true;
                }

            return false;
        }

        bool environmentLeft()
        {
            Vector3 left = transform.TransformDirection(Vector3.left);
            if (Physics.Raycast(transform.position, left, 5))
            {
                return true;
            }

            return false;


        }


        bool environmentRight()
        {
            Vector3 right = transform.TransformDirection(Vector3.right);
            if (Physics.Raycast(transform.position, right, 5))
            {
                return true;
            }

            return false;


        }


        //node to  move randomly until the player is in sight
        /*
        private Node Patrol()
        {
            
            return new Action(() =>  ));


        }
        */
        //calculates the angle from the enemy to its target and turns toward that angle
        private Node TurnToPlayer()
        {
            Vector3 targetPos = TargetTransform().position;
            Vector3 targetDir = targetPos - transform.position;
            float angle;
            angle = Vector3.Angle(targetDir, transform.forward );
            print(angle);

            return new Action(() => Turn(angle));

        }



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


        private Node WeakFire()
        {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 0.5f)));
        }



        private Root fun()
        {
            return new Root(
            new Service(0.2f, UpdatePerception,
            new Selector(

            //if there is no target in front of the enemy then: move forward
            new BlackboardCondition("targetOpen",
                        Operator.IS_EQUAL, false,
                        Stops.IMMEDIATE_RESTART,
                        new Selector(


                            //move forward until there is part of the environment. if there is then turn right
                            /*new BlackboardCondition("environmentRight",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                        new Action(() => Move(0.5f))

                                                        )
                                ),

                            new BlackboardCondition("environmentLeft",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                        new Action(() => Move(0.5f))
                                                        )

                                ),
                                */
                                //need to fix when it gets stuck
                            new BlackboardCondition("environmentFront",
                                                    Operator.IS_EQUAL, true,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Selector(
                                                        new BlackboardCondition("environmentRight",
                                                                                Operator.IS_EQUAL, true,
                                                                                Stops.IMMEDIATE_RESTART,
                                                                                new Sequence(
                                                                                    StopMove(),
                                                                                    new Action(() => Turn(-0.7f))
                                                                                    )
                                                            ),
                                                        new BlackboardCondition("environmentLeft",
                                                                                Operator.IS_EQUAL,true,
                                                                                Stops.IMMEDIATE_RESTART,
                                                                                new Sequence(
                                                                                    StopMove(),
                                                                                    new Action(() => Turn(0.7f))
                                                                                    )
                                                            
                                                            ),
                                                        new Sequence(
                                                            new Action(() => Move(-0.5f)),
//                                                            new Wait(0.5f),
                                                            new Action(() => Turn(0.7f))
                                                            )
                                                    )
                                ),
                            new Sequence(
                                StopTurning(),
                                new Action(() => Move(0.5f))
                                )
                            //turn left at all times until a condition is met
                            //new Action(() => Turn(-0.7f))


                                        
                        )
                ),
                
            // if the distance of the target is less that 10 meters, move forward and turn toward the player until within a certain distance.
            new BlackboardCondition("targetDistance",
                                    Operator.IS_GREATER, 10f,
                                    Stops.IMMEDIATE_RESTART,
                                    new Selector(
                                        new BlackboardCondition("targetInFront",
                                                                Operator.IS_EQUAL, false,
                                                                Stops.IMMEDIATE_RESTART,
                                                                new Selector(
                                                                    //if the player is not in front of the enemy, turn until it is in front of the enemy
                                                                    new BlackboardCondition("targetOnRight",
                                                                                            Operator.IS_EQUAL, true,
                                                                                            Stops.IMMEDIATE_RESTART,
                                                                                            new Sequence(
                                                                                                StopMove(),
                                                                                                new Action(() => Turn(0.7f))
                                                                                                )

                                                                        ),
                                                                    new BlackboardCondition("targetOnRight",
                                                                                            Operator.IS_EQUAL, false,
                                                                                            Stops.IMMEDIATE_RESTART,
                                                                                            new Sequence(
                                                                                                StopMove(),
                                                                                                new Wait(0.5f),
                                                                                                new Action(() => Turn(-0.7f))
                                                                                                )
                                                                        )
                                                                   )

                                                                ),
                                                                // when the targeet is in front of the enemy, turn until the enemy is direcctly facing the target.
                                                                new BlackboardCondition("targetOffCentre",
                                                                                        Operator.IS_SMALLER_OR_EQUAL,0.1f,
                                                                                        Stops.IMMEDIATE_RESTART,
                                                                                        new Sequence(
                                                                                            StopTurning(),
                                                                                            new Action(() => Move(0.5f))
                                                                                            )
                                                                    
                                                                    )
                                            )
                                       ),

            // once the player is in  range, stop and then turn to the player.
            //checks to see if the player is not off centre
            new BlackboardCondition("targetOffCentre",
                                    Operator.IS_GREATER_OR_EQUAL, 0.05f,
                                    Stops.IMMEDIATE_RESTART,
                                    new Selector(                                        
                                        new BlackboardCondition("targetOnRight",
                                                                Operator.IS_EQUAL, true,
                                                                Stops.IMMEDIATE_RESTART,
                                                                new Action(() => Turn(0.7f))
                                            ),
                                        new Action(() => Turn(-0.7f))

                                        )
                                    ),
            new Sequence(
                StopTurning(),
                StopMove(),
                new Action(() => Fire(1f))
                )
            
            



                    )
                )
            );
        }




        /*

        private Root fun()
        {

            // add the AI dodging player shells
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(


                        //checks to see if the player is not bbehind the environment by using raycasting.
                        new BlackboardCondition("targetOpen",
                        Operator.IS_EQUAL, true,
                        Stops.IMMEDIATE_RESTART,
                        //it runs the children sequentially until it succeeds, but also runs it til it fails, if it succeeds to turn right, dont turn left, if it fails to turn right then turn left
                        new Sequence(                                    
                            new Selector(

                                                //if the target is in front of the AI, it moves forward
                                                new BlackboardCondition("targetInFront",
                                                            Operator.IS_EQUAL, true,
                                                            Stops.IMMEDIATE_RESTART,
                                                            new Action(() => Move(0.5f))
                                                            ),                                                    



                                                // of the target is on the right of the AI, it turns right
                                                new BlackboardCondition("targetOnRight",
                                                                        Operator.IS_EQUAL, true,
                                                                     Stops.IMMEDIATE_RESTART,

                                                                     new Sequence(
                                               // Turn right toward target
                                               StopMove(),
                                               new Action(() => Turn(0.7f))
                                                    )
                                               ),


                                               // when the target is in front of the AI, the Ai stops moving and turning and shoots
                                               new BlackboardCondition("targetOffCentre",
                                                    Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                    Stops.IMMEDIATE_RESTART,
                                                    // Stop turning and fire
                                                    new Sequence(
                                                                StopTurning(),
                                                                StopMove(),
                                                                //new Action(() => Fire(0.5f)),
                                                                new Wait(2f),


                                                // of the target is on the right of the AI, it turns right
                                                new BlackboardCondition("targetOnRight",
                                                                        Operator.IS_EQUAL, false,
                                                                     Stops.IMMEDIATE_RESTART,
                                               // Turn right toward target
                                               new Action(() => Turn(-0.7f))),

                                                StopMove()

                                            )
                                         )

                                                    /*
                                                    new BlackboardCondition("targetDistance",
                                                    Operator.IS_SMALLER, 10f,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                    
                                                    

                                                                )
                                                            )
                                                        
                                                    )

                                 )
                            ),

                                                    
                            new Sequence(
                        //if the target is 20 meters or more from the player it checks to see if the environment blavkboard is true
                        new BlackboardCondition("targetDistance",
                                                    Operator.IS_GREATER_OR_EQUAL, 10f,
                                                    Stops.IMMEDIATE_RESTART,
                                                    new Selector(
                        // if the AI reaches a building or part of the envvironment it stops then turns right and then proceeds forward
                        
                        new BlackboardCondition("environmentLeft",
                                               Operator.IS_EQUAL, true,
                                               Stops.IMMEDIATE_RESTART,

                                               new Sequence(
                                               new Action(() => Turn(0.7f)),
                                               new Wait(0.2f),
                                               new Action(() => Turn(-0.7f)),
                                               new Action(() => Move(0.7f))

                                                        )
                                                     ),
                                                    
                        new BlackboardCondition("environmentRight",
                                               Operator.IS_EQUAL, true,
                                               Stops.IMMEDIATE_RESTART,

                                               new Sequence(
                                               new Action(() => Turn(-0.7f)),
                                               new Wait(0.2f),
                                               new Action(() => Turn(0.7f)),
                                               new Action(() => Move(0.7f))

                                                        )
                                                     ), 
                                                     
                        new BlackboardCondition("environmentFront",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,

                                                new Sequence(
                                                new Action(() => Turn(0.7f)),
                                                new Wait(0.2f),
                                                new Action(() => Move(0.7f))

                                                                  )
                                                            )
                                                         )
                                                    )
                                                )
      
                                    )
                                )
                                
                            );


        }
        
    */




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

                            new BlackboardCondition("environmentFront",
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