#region License
/*
 *   This file is part of Orbital Material Science.
 *
 *   Orbital Material Science is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Orbital Material Sciencee is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Orbital Material Science.  If not, see <http://www.gnu.org/licenses/>.
 */
#endregion
/*
 *   This file implements a helper class to hook into the Science Result Dialog.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens.Flight.Dialogs;

namespace NE_Science
{
    /// <summary>
    /// Your class must implement this Interface in order to register with the ExperimentResultDialog class.
    /// </summary>
    public interface IScienceResultHelperClient
    {
        /// <summary>
        /// A valid Part to match against and receive the event Messages.
        /// </summary>
        Part getPart();
    }

    /// <summary>
    /// This class is used to hook into the ExperimentResultsDialog which pops up whenever an experiment is completed.
    /// </summary>
    /// Usage: Register your class here to receive callbacks when the user interacts with the ExperimentResultsDialog.
    /// The most useful are :
    ///     OnExperimentResultDialogResetClicked
    ///     OnExperimentResultDialogTransmitClicked
    ///     OnExperimentResultDialogLabClicked
    ///
    /// But you can also be informed about the following :
    ///     OnExperimentResultDialogOpened
    ///     OnExperimentResultDialogClosed
    ///     OnExperimentResultDialogPageChanged
    ///
    /// Your class can implement any or all of the above event handlers.
    ///

    /// /////////////////////////////////////////////////////////////////
    ///
    /// NOT WORKING - a MonoBehaviour assumes it is attached to some GameObject
    /// Have a re-think about how this could work.
    /// 
    /// //////////////////////////////////////////////////////////////////
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class ScienceResultHelper : MonoBehaviour
    {
        /// <summary>
        /// This is our Singleton
        /// </summary>
        private static ScienceResultHelper _instance = null;

        /// <summary>
        /// Return a reference to the ExperimentResultHelper object.
        /// </summary>
        public static ScienceResultHelper Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new ScienceResultHelper();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor prevents anybody else creating another instance
        /// </summary>
        private ScienceResultHelper()
        {
            // Do any non-Unity initialization here?
        }

        private void Awake()
        {
            // Start off disabled
            //enabled = false;
            _instance = this;
            // if once
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            
        }

        /// <summary>
        /// This keeps track of whether or not the ExperimentsResultDialog is open or not.
        /// </summary>
        private ExperimentsResultDialog _dialog = null;
        private ExperimentResultDialogPage _currentPage = null;
        private Dictionary<string, Button> _buttons = null;
        private Dictionary<string, Button> _Buttons
        {
            get
            {
                if(_buttons == null)
                {
                    _buttons = new Dictionary<string, Button>();
                }
                return _buttons;
            }
        }

        private List<IScienceResultHelperClient> _clients = null;
        private List<IScienceResultHelperClient> _Clients
        {
            get {
                if (_clients == null)
                {
                    _clients = new List<IScienceResultHelperClient>();
                }
                return _clients;
            }
        }

        /// <summary>
        /// List of buttons which can be controlled
        /// </summary>
        public enum ExperimentResultDialogButton
        {
            ButtonReset,
            ButtonLab,
            ButtonTransmit,
            ButtonTransmit_CommNet
        };

        /// <summary>
        /// Register a new client to receive callbacks.
        /// </summary>
        /// <param name="f_client"></param>
        public void Register(IScienceResultHelperClient f_client)
        {
            var c = _Clients.AddUnique(f_client);
            if (!(c is null))
            {
                NE_Helper.log("Added " + f_client.ToString() + " as ExperimentResultDialog listener.");
            }
        }

        /// <summary>
        /// Deregister a client
        /// </summary>
        /// <param name="f_client"></param>
        public void Unregister(IScienceResultHelperClient f_client)
        {
            //var index = _Clients.FindIndex(e => e.getPart() == f_client.getPart());
            //_Clients.RemoveAt(index);
            var didRemove = _Clients.Remove(f_client);
            if (didRemove)
            {
                NE_Helper.log("Removed " + f_client.ToString() + " from ExperimentResultDialog listeners.");
            }
            if (_Clients.Count == 0)
            {
                //enabled = false;
            }
        }

        /// <summary>
        /// Sends message to the current client
        /// </summary>
        /// TODO: SendMessage() is VERY inefficient; look into replacing with Events or Delegates
        /// https://unity3d.com/learn/tutorials/topics/performance-optimization/optimizing-scripts-unity-games
        /// On the other hand, this dialog should not be opened very often so we might get away with it.
        private void _SendMessageToClient(string f_message)
        {
            if (_Clients.Exists(c => c.getPart() == _currentPage.host))
            {
                _currentPage.host.SendMessage(f_message, SendMessageOptions.DontRequireReceiver);
            }
        }


        #region ButtonControls
        /// <summary>
        /// Disables a button.
        /// </summary>
        /// Button is greyed out, but still visible on the Dialog.
        /// <param name="f_button">The button to disable.</param>
        public void DisableButton(ExperimentResultDialogButton f_button)
        {
            Button b = _Buttons[f_button.ToString()];
            if (b)
            {
                b.interactable = false;
            }
        }

        /// <summary>
        /// Enables a button.
        /// </summary>
        /// Button is made active.
        /// <param name="f_button">The button to enable.</param>
        public void EnableButton(ExperimentResultDialogButton f_button)
        {
            Button b = _Buttons[f_button.ToString()];
            if(b)
            {
                b.interactable = true;
            }
        }

        /// <summary>
        /// Hides a button.
        /// </summary>
        /// Button is hidden from the dialog.
        /// NOTE: Hiding buttons makes the Result panel in the dialog shrink.
        /// <param name="f_button">The button to hide.</param>
        public void HideButton(ExperimentResultDialogButton f_button)
        {
            Button b = _Buttons[f_button.ToString()];
            if (b)
            {
                b.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Shows a button.
        /// </summary>
        /// Button is shown on the dialog.
        /// <param name="f_button">The button to show.</param>
        public void ShowButton(ExperimentResultDialogButton f_button)
        {
            Button b = _Buttons[f_button.ToString()];
            if (b)
            {
                b.gameObject.SetActive(false);
            }
        }
        #endregion

        #region ButtonCallbacks
        protected void OnResetClicked()
        {
            _SendMessageToClient("OnExperimentResultDialogResetClicked");
        }

        protected void OnLabClicked()
        {
            _SendMessageToClient("OnExperimentResultDialogLabClicked");
        }

        protected void OnTransmitClicked()
        {
            _SendMessageToClient("OnExperimentResultDialogTransmitClicked");
        }

        protected void OnTransmitCommNetClicked()
        {
            _SendMessageToClient("OnExperimentResultDialogTransmitCommNetClicked");
        }
        #endregion

        #region ExperimentsResultDialog Callbacks
        /// <summary>
        /// Called when the ExperimentResultDialog page changes, including when it is first opened.
        /// </summary>
        protected void OnExperimentResultDialogPageChanged(ExperimentResultDialogPage oldPage)
        {
            // TODO: Perhaps need to reset the old page or something?
            _SendMessageToClient("OnExperimentResultDialogPageChanged");
        }

        /// <summary>
        /// Called in the frame that the ExperimentResultDialog opened
        /// </summary>
        protected void OnExperimentResultDialogOpened()
        {
            ExperimentsResultDialog erd = ExperimentsResultDialog.Instance;

            UnityEngine.UI.Button[] buttons = erd.GetComponentsInChildren<UnityEngine.UI.Button>();
            foreach (UnityEngine.UI.Button b in buttons)
            {
                switch(b.name)
                {
                    case "ButtonReset":
                        b.onClick.AddListener(OnResetClicked);
                        break;
                    case "ButtonLab":
                        b.onClick.AddListener(OnLabClicked);
                        break;
                    case "ButtonTransmit":
                        b.onClick.AddListener(OnTransmitClicked);
                        break;
                    case "ButtonTransmit_CommNet":
                        b.onClick.AddListener(OnTransmitCommNetClicked);
                        break;
                }
                _Buttons[b.name] = b;

                // Disable buttons
                //if (b.name == "ButtonReset" || b.name == "ButtonLab" || b.name == "ButtonTransmit" || b.name == "ButtonTransmit_CommNet")
                //{
                // Do this to make the button greyed-out
                //b.interactable = false;
                // Do this to hide the button
                //b.gameObject.SetActive(false);
                // Do this to hook into the button onClick event
                //b.onClick.AddListener(ResetExperiment);

                // NB: Hiding too many buttons makes the review window too small; so either need to resize it manually,
                // or just grey-out the buttons.
                //}
                //b.onClick.AddListener(() => OnExperimentResultDialogClicked(b.name));
            }

            // This doesn't seem to do anything
            //erd.currentPage.showReset = false;

            _SendMessageToClient("OnExperimentResultDialogOpened");

        }

        /// <summary>
        /// Called in the frame that the ExperimentResultDialog closed
        /// </summary>
        protected void OnExperimentResultDialogClosed()
        {
            _Buttons.Clear();
            _SendMessageToClient("OnExperimentResultDialogClosed");
        }
        #endregion

        /// <summary>
        /// Gets called every frame; drives the callbacks in this class.
        /// </summary>
        public void Update()
        {
            // NB: We have to use 'is' because of Unity's overload of "== null" and the
            // way it marks destroyed objects. The alternative would be to use a boolean
            // instead of reference variable.
            if (! (_dialog is null))
            {
                if (ExperimentsResultDialog.Instance == null)
                {
                    // Dialog closed this frame; call the relevant callbacks
                    OnExperimentResultDialogClosed();
                    _currentPage = null;
                    _dialog = null;
                }
                else
                {
                    if (_dialog.currentPage != _currentPage)
                    {
                        // Dialog changed page this frame; call the relevant callbacks
                        _currentPage = _dialog.currentPage;
                        OnExperimentResultDialogPageChanged(null);
                    }

                    // Perform any other logic every frame the dialog is open here.
                }
            }
            if (_dialog is null && ExperimentsResultDialog.Instance != null)
            {
                // Dialog opened this frame; call the relevant callbacks
                _dialog = ExperimentsResultDialog.Instance;
                var oldPage = _currentPage;
                _currentPage = _dialog.currentPage;
                OnExperimentResultDialogOpened();
                OnExperimentResultDialogPageChanged(oldPage);
            }
        }
    } // class ScienceResultHelper
} // Namespace NE_Science
