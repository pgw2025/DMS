# PMSWPF

## OPC UA Service

This project includes an OPC UA service implementation that provides the following functionalities:

### Features
- Connect to OPC UA servers
- Browse nodes in the OPC UA address space
- Read and write values from/to OPC UA nodes
- Add subscriptions for monitoring node changes

### Usage

```csharp
// Create an instance of the OPC UA service
var opcUaService = new OpcUaService();

// Connect to an OPC UA server
await opcUaService.CreateSession("opc.tcp://localhost:4840");

// Check connection status
if (opcUaService.IsConnected())
{
    // Browse nodes
    var rootNodeId = ObjectIds.RootFolder;
    var references = opcUaService.BrowseNodes(rootNodeId);
    
    // Read a value
    var value = opcUaService.ReadValue(someNodeId);
    
    // Write a value
    opcUaService.WriteValue(someNodeId, newValue);
    
    // Add a subscription
    var subscription = opcUaService.AddSubscription("MySubscription");
}

// Disconnect when done
opcUaService.Disconnect();
```

### Testing

Unit tests for the OPC UA service are included in the `DMS.Infrastructure.UnitTests` project. Run them using your preferred test runner.

## Trigger System

The trigger system has been updated to support associating triggers with multiple variables instead of just one. This allows for more flexible trigger configurations where a single trigger can monitor multiple variables.

### Key Changes
- Modified `TriggerDefinition` to use a list of variable IDs instead of a single variable ID
- Added a new `TriggerVariables` table to maintain the many-to-many relationship between triggers and variables
- Updated the UI to support selecting multiple variables when creating or editing triggers
- Updated all related services and repositories to handle the new many-to-many relationship