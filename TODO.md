# SysMax TODO List

## User-Reported Items

- [-] Basic User Mode: Still showing too many tools. *(Implemented hiding Nav buttons)*
- [X] System Information/Hardware Page: Memory/RAM free is showing 0.0 GB. *(Fixed: Added AvailableRAM to service data)*
- [X] Help & Support Page (Video Tutorials): Add intro to the service and an example of a solution. *(Done)*

## Code Comments

- [X] `EnhancedHardwareMonitorService.cs:504`: Possible conversion issue? `double temp = Convert.ToDouble(obj["CurrentTemperature"]);` *(Made conversion robust)*
- [-] `ApplicationSettingsPage.xaml.cs:99`: Implement update checking functionality. *(Deferred for simplification)*
- [X] `ApplicationSettingsPage.xaml.cs:110`: Get version dynamically from assembly info instead of hardcoding. *(Done)*
- [-] `ApplicationSettingsPage.xaml.cs:190`: Implement language selection mapping if `SetLanguageFromIndex` is used. *(Deferred for simplification)*
- [-] `ApplicationSettingsPage.xaml.cs:209`: Implement immediate application of settings like theme/update frequency. *(Deferred for simplification)*
- [X] `MainWindow.xaml.cs:94`: Replace placeholder hardware values with actual values from LibreHardwareMonitorLib. *(Done)*
- [X] `MainWindow.xaml.cs:104`: Address the TODO item (context needed). *(Done - Status bar placeholders replaced)*
- [-] `MainWindow.xaml.cs:188`: Implement logic to show/hide UI elements based on user mode. *(Done for Nav buttons)*
- [X] `Controls/NumericStringConverter.cs:58`: Implement the `ConvertBack` method if needed. *(Implemented)* 