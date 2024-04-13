using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Input;
using System.Diagnostics;

namespace SharpBlunamiControl
{
    public partial class BlunamiControl
    {

        // ============================================================================
        // Surface Dial Elements
        // ============================================================================
        RadialController radialController; // Surface Dial Instance
        RadialControllerMenuItem radialControllerTrainControlItem;
        RadialControllerMenuItem radialControllerTrainHornItem;
        RadialControllerMenuItem radialControllerTrainIncrementalSpeedItem;
        IntPtr handle;
        // ============================================================================
        // Surface Dial Setup
        // ============================================================================

        private void SurfaceDialSetup()
        {
            handle = Process.GetCurrentProcess().MainWindowHandle;
            Console.WriteLine("Creating Surface Dial instance...");
            CreateController();
            Console.WriteLine("Subscribing to RadialController callbacks...");
            SubscribeToControllerCallbacks();
            Console.WriteLine("Adding custom items...");
            AddCustomItems();
            Console.WriteLine("Setting default items...");
            //SetDefaultItems();
            Console.WriteLine("Surface Dial setup done...");
        }
        private void CreateController()
        {
            IRadialControllerInterop interop = (IRadialControllerInterop)System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMarshal.GetActivationFactory(typeof(RadialController));
            Guid guid = typeof(RadialController).GetInterface("IRadialController").GUID;

            
            radialController = interop.CreateForWindow(handle, ref guid);
        }

        private void SubscribeToControllerCallbacks()
        {
            radialController.ButtonClicked += RadialController_ButtonClicked;
            radialController.ControlAcquired += RadialController_ControlAcquired;
            radialController.RotationChanged += RadialController_RotationChanged;
            radialController.ButtonHolding += RadialController_ButtonHolding; // only works if IsMenuSuppresed == true
            radialController.ScreenContactStarted += RadialController_ScreenContactStarted;
            radialController.ScreenContactContinued += RadialController_ScreenContactContinued;
        }

        private void SetDefaultItems()
        {
            RadialControllerConfiguration radialControllerConfig;
            IRadialControllerConfigurationInterop radialControllerConfigInterop = (IRadialControllerConfigurationInterop)System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMarshal.GetActivationFactory(typeof(RadialControllerConfiguration));
            Guid guid = typeof(RadialControllerConfiguration).GetInterface("IRadialControllerConfiguration").GUID;

            radialControllerConfig = radialControllerConfigInterop.GetForWindow(handle, ref guid);
            radialControllerConfig.SetDefaultMenuItems(new[] { RadialControllerSystemMenuItemKind.Volume });
            radialControllerConfig.TrySelectDefaultMenuItem(RadialControllerSystemMenuItemKind.Volume);
        }

        private void AddCustomItems()
        {
            AddTrainControlItems();
            AddItemFromImage();
        }

        private void AddTrainControlItems()
        {
            // First create our Train tools with predefined Icons.
            radialControllerTrainHornItem = RadialControllerMenuItem.CreateFromFontGlyph("Active Train Horn/Bell", "\xD83D\xDCEF", "Segoe UI Emoji");
            radialControllerTrainIncrementalSpeedItem = RadialControllerMenuItem.CreateFromFontGlyph("Active Engine Incremental Speed Adjustment", "\xD83D\xDE82", "Segoe UI Emoji");
            radialController.Menu.Items.Add(radialControllerTrainHornItem);
            radialController.Menu.Items.Add(radialControllerTrainIncrementalSpeedItem);
        }

        private void AddItemFromImage()
        {
            // Use a different approach from the Microsoft samples since that didn't work.
            string iconFilePath = Path.Combine(Directory.GetCurrentDirectory(), "graphics\\tmcc64.png");
            var getItemImageOperation = StorageFile.GetFileFromPathAsync(iconFilePath);
            getItemImageOperation.Completed += new AsyncOperationCompletedHandler<StorageFile>(OnImageFileFound);
        }

        private void OnImageFileFound(IAsyncOperation<StorageFile> asyncInfo, AsyncStatus asyncStatus)
        {
            if (asyncStatus == AsyncStatus.Completed)
            {
                StorageFile imageFile = asyncInfo.GetResults();
                radialControllerTrainControlItem = RadialControllerMenuItem.CreateFromIcon("Active Engine Control", RandomAccessStreamReference.CreateFromFile(imageFile));
                radialController.Menu.Items.Add(radialControllerTrainControlItem);
            }
        }

        // ============================================================================
        // Surface Dial Events
        // ============================================================================

        private void RadialController_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            
        }

        private void RadialController_ControlAcquired(RadialController sender, RadialControllerControlAcquiredEventArgs args)
        {

        }

        private void RadialController_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {

        }

        private void RadialController_ButtonHolding(RadialController sender, RadialControllerButtonHoldingEventArgs args)
        {

        }


        // ============================================================================
        // Surface Dial Screen contact NOT WORKING.
        // ============================================================================

        private void RadialController_ScreenContactStarted(RadialController sender, RadialControllerScreenContactStartedEventArgs args)
        {
            RadialControllerScreenContact screenContact = args.Contact;
        }

        private void RadialController_ScreenContactContinued(RadialController sender, RadialControllerScreenContactContinuedEventArgs args)
        {
            RadialControllerScreenContact screenContact = args.Contact;
        }
    }
}
