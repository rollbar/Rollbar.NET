Imports Rollbar

Module Module1

    Sub Main()

        'Configure and initialize the Rollbar infrastructure
        Dim rollbarConfig As RollbarInfrastructureConfig = New RollbarInfrastructureConfig("17965fa5041749b6bf7095a190001ded", "RollbarNetSamples")
        RollbarInfrastructure.Instance.Init(rollbarConfig)

        'Now, we can start using the shared logger as we wish...

        'Send the very first info message to the Rollbar API Server
        RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Info("Sample.VBNet.ConsoleApp: Rollbar SDK is initialized!")

        'Now let's send a simulated exception:
        Try
            Throw New Exception("Sample.VBNet.ConsoleApp: Crashed and burnt!")
        Catch ex As Exception
            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Critical(ex)
        End Try

    End Sub

End Module
