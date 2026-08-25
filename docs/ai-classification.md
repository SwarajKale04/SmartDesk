# AI ticket classification

SmartDesk uses `ITicketClassificationService` so ticket workflows do not depend on a particular ML implementation. The initial `MlNetTicketClassificationService` trains lightweight SDCA multiclass models in-process from a synthetic training set (336 examples: seven categories × four priorities × twelve variations).

On ticket creation, the service predicts a category, priority, and confidence. Predictions at or above **60%** are applied; predictions below **80%** are flagged for human review. Below 60%, the ticket retains the customer-selected priority and category is not auto-applied. Any classifier failure is recorded in audit history but never prevents ticket creation.

This seed model is an interview-ready automation baseline, not a claim of production accuracy. Replace it with a versioned, evaluated ML.NET/ONNX model before production use, and add a human-feedback loop to improve its training data.
