using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {
        private Root CreateBehaviourTree() {

            switch (m_Behaviour) {

                case 0:
                    return fun();
                case 1:
                    return deadly(-0.05f, 1f);
                case 2:
                    return frightened();
                case 3:
                    return unpredictable();
                default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */
        private void UpdatePerception() {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;
            blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
        }

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }

        //MOVE!!
        
            private Node StopMove()
        {
            return new Action(() => Move(0));

        }


        //FIRE!!!
        private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

        private Node WeakFire(){
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 0.5f)));
        }



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
                                                    Operator.IS_GREATER_OR_EQUAL, 10f,
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


        



        /* Example behaviour trees */

        // Constantly spin and fire on the spot 
        private Root deadly(float turn, float shoot) {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        
    }

        // Turn to face your opponent and fire
        private Root frightened() {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        new Wait(2f),
                                        RandomFire())),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(0.2f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.2f))
                    )
                )
            );
        }


private Root unpredictable()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        new Wait(2f),
                                        RandomFire())),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(0.2f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.2f))
                    )
                )
            );
        }




    }
}