pipeline {
    agent none

    options {
        timestamps()

        buildDiscarder(logRotator(
            numToKeepStr: '10',
            daysToKeepStr: '3'
        ))
    }

    stages {
        stage('BuildAndTest') {
            matrix {
                agent none
                axes {
                    axis {
                        name 'CONFIGURATRION'
                        values 'Debug', 'Release'
                    }
                    axis {
                        name 'ZLINQ'
                        values 'true', 'false'
                    }
                    axis {
                        name 'MEMORYPACK'
                        values 'true', 'false'
                    }
                    axis {
                        name 'NEWTONSOFTJSON'
                        values 'true', 'false'
                    }
                }
                stages {
                    stage('Build') {
                        agent {
                            docker {
                                image 'mcr.microsoft.com/dotnet/sdk:9.0'
                                label 'docker'
                            }
                        }                        
                        steps {
                            echo "Build for ${CONFIGURATRION}: ZLINQ = ${ZLINQ}, MEMORYPACK = ${MEMORYPACK}, NEWTONSOFTJSON = ${NEWTONSOFTJSON}"
                            sh '/usr/bin/dotnet dotnet build -c ${CONFIGURATRION} -p:EnableZLinq=${ZLINQ} -p:EnableMemoryPack=${MEMORYPACK} -p:EnableNewtonsoftJson=${NEWTONSOFTJSON}'
                        }
                    }
                    // TODO: tests
                }
            }
        }
    }
}
