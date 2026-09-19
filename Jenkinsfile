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
                agent {
                    label 'dotnet10'
                }

                axes {
                    axis {
                        name 'CONFIGURATION'
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
                        steps {
                            echo "Build for ${CONFIGURATION}: ZLINQ = ${ZLINQ}, MEMORYPACK = ${MEMORYPACK}, NEWTONSOFTJSON = ${NEWTONSOFTJSON}"

                            sh """
                                dotnet build \
                                    -c ${CONFIGURATION} \
                                    -p:EnableZLinq=${ZLINQ} \
                                    -p:EnableMemoryPack=${MEMORYPACK} \
                                    -p:EnableNewtonsoftJson=${NEWTONSOFTJSON}
                            """
                        }
                    }

                    // TODO: tests
                }
            }
        }
    }
}