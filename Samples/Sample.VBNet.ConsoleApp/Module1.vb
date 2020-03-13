Imports Rollbar

Module Module1

    Sub Main()

        'Configure and initialize the Rollbar shared singleton-like logger
        Dim rollbarConfig As RollbarConfig = New RollbarConfig("17965fa5041749b6bf7095a190001ded")
        rollbarConfig.Environment = "RollbarNetSamples"
        RollbarLocator.RollbarInstance.Configure(rollbarConfig)

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
