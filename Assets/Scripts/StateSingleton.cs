using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StateSingleton {
   static StateSingleton()
    {
        stateView = StateView.UNSET;
        stateMode = StateMode.UNSET;
    }

    public enum StateView { UNSET, MODE2D, MODE2D_PLUS_3DVP, MODE2D_PLUS_OCULUS, MODE2D_PLUS_3DVP_PLUS_OCULUS };
    public enum StateMode { UNSET, WALKING, FLIGHT, DRONE};

    public static StateView stateView;
    public static StateMode stateMode;

}
