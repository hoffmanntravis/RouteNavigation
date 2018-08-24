using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;

namespace RouteNavigation
{
    public class GeneticAlgorithm
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        static protected int iterations = 2000;
        static public int populationSize = 4000;
        static public int neighborCount = 120;
        static public int tournamentSize = 80;
        static public int tournamentWinnerCount = 1;
        static public int breedersCount = 4;
        static public int offSpringPoolSize = 1;
        static public double crossoverProbability = .35;

        static public double elitismRatio = .005;
        static public double mutationProbability = .2;
        static public int mutationAlleleMax = 2;
        static public double growthDecayExponent = 1;
        static public bool toggleIterationsExponent = true;
        static protected int currentIteration = 0;

        protected Config config;
        static protected List<Location> allLocations = DataAccess.GetLocations();
        static protected List<Location> possibleLocations;
        static protected List<Vehicle> allVehicles = DataAccess.GetVehicles();
        List<Vehicle> availableVehicles;
        static private object lockObject = new object();

        static private Random rng = new Random();

        public List<List<Location>> initializePopulation(int populationSize)
        {
            if (populationSize < 2)
            {
                Exception exception = new Exception("Population Size must be at least two.  Otherwise, genetic operations and crossover are not possible.  Please increase the value of this parameter.");
                throw exception;
            }
            if (breedersCount < 2)
            {
                Exception exception = new Exception("Breeders Count must be at least two.  Otherwise, genetic operations and crossover are not possible.  Please increase the value of this parameter.");
                throw exception;
            }
            List<List<Location>> startingPopulation = new List<List<Location>>();
            Logger.Info("Creating a randomized starting Population of locations, pool size:" + populationSize);

            for (int x = 0; x < populationSize; x++)
            {
                startingPopulation.Add(new List<Location>(possibleLocations).Shuffle(rng).ToList());
            }

            return startingPopulation;
        }

        public bool testInitializePopulation()
        {
            int unitPopulation = 5;
            List<List<Location>> testPopulation = initializePopulation(unitPopulation);
            if (testPopulation.Count != unitPopulation)
                return false;

            foreach (List<Location> locations in testPopulation)
            {
                if (locations.Count != unitPopulation)
                    return false;
                foreach (Location location in locations)
                {
                    if (location.address is null)
                        return false;
                    if (location.neighbors.Count == 0)
                        return false;
                }
            }
            return true;
        }

        public void detectDuplicates(List<RouteCalculator> calcs)
        {
            List<RouteCalculator> duplicateCalcs = calcs.GroupBy(c => c.GetHashCode()).Where(g => g.Skip(1).Any()).SelectMany(c => c).ToList();
            int duplicatesCount = duplicateCalcs.Count;

            Logger.Info("There are " + duplicateCalcs.Count + " Duplicates");
        }

        public void calculateBestRoutes()
        {
            config = DataAccess.GetConfig();
            if (Config.Calculation.origin == null)
            {
                string errorMessage = "Please set the origin location id in the config page before proceeding.  This should correspond to a location id in the locations page.";
                Logger.Error(errorMessage);
                Exception e = new Exception(errorMessage);
                throw e;
            }

            Logger.Info(String.Format("There are {0} locations in the database that could potentially be processed.", allLocations.Count));
            //Calcualte the distance from source to depot for every instance.  This will not change, so do it ahead of time.  Can probably be moved into the constructor.
            availableVehicles = DataAccess.GetVehicles().Where(v => v.operational == true).ToList();
            possibleLocations = allLocations.ToList();
            possibleLocations.ForEach(l => l.distanceFromDepot = RouteCalculator.CalculateDistance(Config.Calculation.origin, l));
            possibleLocations = possibleLocations.Except(possibleLocations.Where(a => a.coordinates.lat is double.NaN || a.coordinates.lng is double.NaN)).ToList();
            possibleLocations = RouteCalculator.GetPossibleLocations(availableVehicles, possibleLocations);
            //remove the origin from all locations since it's only there for routing purposes and is not part of the set we are interested in
            possibleLocations.RemoveAll(s => s.address == Config.Calculation.origin.address);

            Logger.Info(String.Format("After filtering locations based on distance, overdue status, unpopulated GPS coordinates, and removing the origin, {0} locations will be processed", possibleLocations.Count));

            List<List<Location>> startingPopulation = initializePopulation(populationSize);
            //create a batch id for identifying a series of routes calculated together
            int batchId = DataAccess.GetNextRouteBatchId();
            DataAccess.InsertRouteBatch();

            //spin up a single calc to update the data in the database.  We don't want to do this in the GA thread farm since it will cause blocking and is pointless to perform the update that frequently

            //generalCalc.UpdateDistanceFromSource(allLocations);
            //generalCalc.UpdateMatrixWeight(allLocations);
            DataAccess.UpdateDaysUntilDue();

            List<RouteCalculator> fitnessCalcs = new List<RouteCalculator>();

            Logger.Info("Threading intialized locations pool into calculation class instances");

            fitnessCalcs = threadCalculations(startingPopulation, fitnessCalcs);

            Logger.Info("Created random locations pool");

            fitnessCalcs.SortByDistanceAsc();
            double shortestDistanceBasePopulation = fitnessCalcs.First().metadata.routesLengthMiles;

            int emptyCount = fitnessCalcs.Where(c => c.metadata.routesLengthMiles is Double.NaN).Count();
            Logger.Debug(string.Format("There are {0} empty calcs in terms of routesLengthMiles at the outset", emptyCount));

            Logger.Info(string.Format("Base population shortest distance is: {0}", shortestDistanceBasePopulation));

            for (int i = 0; i < iterations; i++)
            {
                Logger.Info(string.Format("Beginning iteration {0}", i));
                currentIteration = i;
                fitnessCalcs = GeneticAlgorithmFitness(fitnessCalcs);

                fitnessCalcs.SortByDistanceAsc();
                double shortestDistance = fitnessCalcs.First().metadata.routesLengthMiles;
                emptyCount = fitnessCalcs.Where(c => c.metadata.routesLengthMiles is Double.NaN).Count();
                Logger.Debug(string.Format("There are {0} empty calcs in terms of routesLengthMiles", emptyCount));
                Logger.Info(string.Format("Iteration {0} produced a shortest distance of {1}.", i + 1, shortestDistance));
            }

            //fully optimized the GA selected route with 3opt swap
            RouteCalculator bestCalc = fitnessCalcs.First();
            foreach (Route route in bestCalc.routes)
                bestCalc.calculateTSPRouteTwoOpt(route);
            DataAccess.insertRoutes(batchId, bestCalc.routes, bestCalc.activityId);
            Logger.Info(string.Format("Final output after 2opt produced a distance of {0}.", bestCalc.metadata.routesLengthMiles));
            DataAccess.UpdateRouteMetadata(batchId, bestCalc.metadata);

            Logger.Info("Finished calculations.");

        }

        public List<RouteCalculator> GeneticAlgorithmFitness(List<RouteCalculator> calcs)
        {
            int eliteCount = Convert.ToInt32(Math.Round(elitismRatio * calcs.Count()));
            if (eliteCount < 1 && elitismRatio > 0)
                eliteCount = 1;
            calcs.SortByDistanceAsc();
            List<RouteCalculator> elites = new List<RouteCalculator>();
            elites.AddRange(calcs.Take(eliteCount));
            calcs.RemoveRange(0, eliteCount);
            Logger.Trace(String.Format("Preserving {0} elites", eliteCount));

            List<RouteCalculator> breeders = geneticSelection(calcs);

            Logger.Trace(string.Format("breeders count is: {0}", breeders.Count));

            List<List<Location>> offspring = produceOffspring(breeders);

            Logger.Trace(string.Format("Offspring count is: {0}", offspring.Count));

            //Add in mutated offspring
            Logger.Trace(string.Format("running potential mutation of {0} offspring", offspring.Count));
            offspring = geneticMutation(offspring);

            calcs = threadCalculations(offspring, calcs);

            /*List<List<Location>> randomSampleMutate = new List<List<Location>>();
            int mutationCount = Convert.ToInt32(Math.Round(mutationProbability * decayFunction() * calcs.Count));
            if (mutationCount > 0)
            {
                for (int x = 0; x < mutationCount; x++)
                {
                    randomSampleMutate.Add(calcs[rng.Next(calcs.Count)].metadata.processedLocations.ToList());
                }
            }

            randomSampleMutate = geneticMutation(randomSampleMutate);
            calcs = threadCalculations(randomSampleMutate, calcs);
            
            Logger.LogMessage(string.Format("running potential mutation of {0} random sample of all calcs", mutationCount), "DEBUG");
            */
            //foreach (RouteCalculator c in calcs)
            //    Logger.LogMessage("CalcCount after mutation: " + c.metadata.processedLocations.Count.ToString());

            //add elites into the list since lower performing items will be removed in place of them
            elites.ForEach(a => calcs.Add(a));

            //Logger.Log(String.Format("{0} elites preserved.", elites.Count));
            //remove the worst performers relative to the mutated offspring count and the elite count that was preserved
            calcs.SortByDistanceDesc();
            for (int x = 0; x < offspring.Count; x++)
                calcs.Remove(calcs.First());

            Logger.Trace(String.Format("Pool size is: {0}", calcs.Count));

            return calcs;
        }

        public RouteCalculator runCalculations(List<Location> list)
        {
            RouteCalculator calc = new RouteCalculator(config, allLocations, possibleLocations, availableVehicles);
            calc.neighborCount = neighborCount;
            calc.CalculateRoutes(list);
            return calc;
        }

        public List<RouteCalculator> threadCalculations(List<List<Location>> locationsList, List<RouteCalculator> calcs)
        {

            SynchronizedCollection<Thread> threads = new SynchronizedCollection<Thread>();
            foreach (List<Location> locations in locationsList)
            {
                Thread thread = new Thread(Action =>
                {
                    RouteCalculator c = runCalculations(locations);
                    lock (lockObject)
                        calcs.Add(c);
                });

                thread.Priority = ThreadPriority.BelowNormal;
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread t in threads)
                t.Join();

            return calcs;
        }

        public List<RouteCalculator> geneticSelection(List<RouteCalculator> parents)
        {
            Logger.Trace(string.Format("Performing genetic selection from {0} parents", parents.Count));
            Logger.Trace(string.Format("Parent metadata for parent 0 is processed locations: {0}, orphaned locations: {1} ", parents[0].metadata.processedLocations.Count, parents[0].metadata.orphanedLocations.Count));

            List<RouteCalculator> breeders = new List<RouteCalculator>();
            while (breeders.Count < breedersCount)
            {
                List<RouteCalculator> contestants = new List<RouteCalculator>();
                for (int x = 0; x < tournamentSize; x++)
                {
                    int randomIndex = rng.Next(parents.Count);
                    RouteCalculator contestant = parents[randomIndex];
                    contestants.Add(contestant);
                }
                List<RouteCalculator> winners = runTournament(contestants);
                foreach (RouteCalculator winner in winners)
                    breeders.Add(winner);
            }
            Logger.Trace(string.Format("Parent metadata for parent 0 is processed locations: {0}, orphaned locations: {1} ", parents[0].metadata.processedLocations.Count, parents[0].metadata.orphanedLocations.Count));
            Logger.Trace(string.Format("Breeders metadata for breeders 0 is processed locations: {0}, orphaned locations: {1} ", breeders[0].metadata.processedLocations.Count, parents[0].metadata.orphanedLocations.Count));

            return breeders;
        }

        public List<List<Location>> produceOffspring(List<RouteCalculator> breeders)
        {

            Logger.Trace(string.Format("Producing offspring from {0} breeders", breeders.Count));
            List<List<Location>> offspring = new List<List<Location>>();
            //make a copy of the original list.  We'll remove from breeders as we populate offspring, but if we need more breeders and run out, we'll refresh the pool with the oirignals.
            breeders.SortByDistanceAsc();
            List<RouteCalculator> breedersoriginal = new List<RouteCalculator>(breeders);

            while (offspring.Count < offSpringPoolSize)
            {
                //Check to ensure there are breeders left in the pool to create offspring, otherwise reset to the original list
                if (breeders.Count == 0)
                {
                    breeders = breedersoriginal.ToList();
                }

                //distribute the next selected index with a growth bias towards lower distance values as sorted above.  That is, put bias of reproduction towards strong breeders.
                int randomIndex = Convert.ToInt32(Math.Floor((rng.Next(breeders.Count)) * (Math.Pow(rng.NextDouble(), growthFunction()))));
                //int randomIndex = rng.Next(breeders.Count);
                RouteCalculator parentA = breeders[randomIndex];
                breeders.Remove(parentA);
                //randomIndex = rng.Next(breeders.Count);
                randomIndex = Convert.ToInt32(Math.Floor((rng.Next(breeders.Count)) * (Math.Pow(rng.NextDouble(), growthFunction()))));
                RouteCalculator parentB = breeders[randomIndex];
                breeders.Remove(parentB);

                List<Location> parentALocations = new List<Location>(parentA.metadata.processedLocations);
                List<Location> parentBLocations = new List<Location>(parentB.metadata.processedLocations);

                double crossoverChance = crossoverProbability * growthFunction();
                Logger.Trace(String.Format("Crossover chance is {0}", crossoverChance));
                if (growthFunction() * rng.Next(101) <= crossoverChance * 100)
                {
                    offspring.Add(geneticCrossoverEdgeRecombine(parentALocations, parentBLocations));
                }
                else
                {
                    //flip a coin and add one of the parents without crossover
                    if (rng.Next(2) == 1)
                        offspring.Add(parentALocations);
                    else
                        offspring.Add(parentBLocations);
                }
                //offspring.Add(geneticCrossover(parentB, parentA));
            }
            Logger.Trace(string.Format("Produced {0} offspring", offspring.Count));
            return offspring;
        }

        /*public List<Location> geneticCrossover(List<Location> parentALocations, List<Location> parentBLocations)
        {

            Logger.LogMessage("Performing genetic crossover from parents", "DEBUG");
      
            int routeHashParentA = generateRouteHash(parentALocations);
            int routeHashParentB = generateRouteHash(parentBLocations);

            if (routeHashParentA != routeHashParentB)
            {
                Logger.LogMessage("routeHashes for ParentA and ParentB do not match");
            }
            

            int crossoverCount = Convert.ToInt32(Math.Round(crossoverRatio * parentALocations.Count));
            if (crossoverRatio > 0 && crossoverCount == 0)
                crossoverCount = 1;
            List<Location> child = new List<Location>();

            int startSwathIndex = rng.Next(parentALocations.Count - crossoverCount);

            //fill the child List with empty locations so we can update existing index locations in future swaps

            List<Location> swathLocations = parentALocations.GetRange(startSwathIndex, crossoverCount);
            //apply the randomely selected swath to the child

            child = swathLocations;

            foreach (Location l in parentBLocations)
            {
                //ignore any locations that were already inserted as part of the swath
                if (child.Contains(l))
                    continue;

                //if the location is on the left of the swath start, insert it at the beginning of the child and increment the index for the next insertion
                if (parentBLocations.IndexOf(l) <= startSwathIndex)
                {
                    child.Insert(0, l);
                }
                //if the location is on the right of the swath start, insert it at the beginning of the child and increment the index for the next insertion
                if (parentBLocations.IndexOf(l) > startSwathIndex)
                {
                    child.Insert(child.Count - 1, l);
                    //wrapAround the leftside if we exceed the index on the right side
                }
            }

            foreach (Location c in child)
                if (c.address == null)
                {
                    int x = 0;
                }

            int childRouteHash = generateRouteHash(child);
            if (routeHashParentA != childRouteHash)
            {
                Logger.LogMessage("routeHashes for ParentA and child do not match");
            }
            else
            {
                    Logger.LogMessage("routeHashes for ParentA and child match");
            }

            if (routeHashParentB != childRouteHash)
            {
                Logger.LogMessage("routeHashes for ParentB and child do not match");
            }
            else
            {
                Logger.LogMessage("routeHashes for ParentB and child match");
            }
            return child;
        }
        */

        public List<Location> geneticCrossoverEdgeRecombine(List<Location> parentALocations, List<Location> parentBLocations)
        {
            //Get Neighbors for the purposes of edge recombination.  Only assign neighbors here since we don't need them elsewhere.
            parentALocations.Where(a => a.neighbors.Count != neighborCount).ToList().ForEach(a => a.neighbors = RouteCalculator.FindNeighbors(a, parentALocations, neighborCount));
            parentBLocations.Where(a => a.neighbors.Count != neighborCount).ToList().ForEach(a => a.neighbors = RouteCalculator.FindNeighbors(a, parentBLocations, neighborCount));
            Logger.Trace("Performing genetic crossover from parents");
            /*

            int routeHashParentA = generateRouteHash(parentALocations);
            int routeHashParentB = generateRouteHash(parentBLocations);

            if (routeHashParentA != routeHashParentB)
            {
                Logger.LogMessage("routeHashes for ParentA and ParentB do not match");
            }

            */
            //work on a copy of the lists.  We don't want to remove their actual neighbors etc

            List<Location> remainingLocations;
            Location x = new Location();
            Location z = new Location();
            int targetRouteCount;
            if (rng.Next(2) == 1)
                remainingLocations = new List<Location>(parentALocations);
            else
                remainingLocations = new List<Location>(parentBLocations);

            targetRouteCount = remainingLocations.Count();
            x = remainingLocations.First();

            List<Location> child = new List<Location>();

            while (child.Count < targetRouteCount)
            {
                child.Add(x);

                //need to break if we hit our target count so we don't index into lists that are empty
                if (child.Count == targetRouteCount)
                    break;
                x.neighbors.Remove(x);
                remainingLocations.Remove(x);
                remainingLocations.ForEach(a => a.neighbors.Remove(x));

                //If x's neighbor list is empty, grab a random node from an intact locations list (ParentALocations) that is not already in child
                if (x.neighbors.Count == 0)
                {
                    z = remainingLocations[rng.Next(remainingLocations.Count)];
                }
                else
                {
                    x.neighbors.Sort((n1, n2) => n1.neighbors.Count.CompareTo(n2.neighbors.Count));
                    //Get a random neighbor that hs the same quantity of neighbors as the first element randomely.  In many cases, this will be the first element, but this is a tie breaker mechanism
                    List<Location> leastNeighborList = x.neighbors.Where(a => a.neighbors.Count == x.neighbors.First().neighbors.Count).ToList();
                    z = leastNeighborList[rng.Next(leastNeighborList.Count)];
                }
                x = z;
            }

            /*
            int childRouteHash = generateRouteHash(child);
            if (routeHashParentA != childRouteHash)
            {
                Logger.LogMessage("routeHashes for ParentA and child do not match");
            }
            else
            {
                Logger.LogMessage("routeHashes for ParentA and child match");
            }

            if (routeHashParentB != childRouteHash)
            {
                Logger.LogMessage("routeHashes for ParentB and child do not match");
            }
            else
            {
                Logger.LogMessage("routeHashes for ParentB and child match");
            }*/
            return child;
        }



        public List<List<Location>> geneticMutation(List<List<Location>> mutateLocationsList)
        {

            List<int> indexes = new List<int>();
            //if rounding causes the pool size to be 0 with a positive mutationRatio, round up to 1
            Logger.Trace(string.Format("Mutation method received {0} locations", mutateLocationsList.Count));

            double mutationChance = mutationProbability * decayFunction();
            //rng.Next is exclusive, so to have probabilities 0 to 100 we need to extend the range to 101
            Logger.Trace(String.Format("Mutation chance is {0}", mutationChance));
            if (rng.Next(101) <= mutationChance * 100)
            {
                foreach (List<Location> mutateLocations in mutateLocationsList)
                {
                    //next couple lines effectively ensure a mutation gene count of at least 1
                    int upperBound = Convert.ToInt32(Math.Round(decayFunction() * mutationAlleleMax));

                    int mutationGeneQuantity = rng.Next(1, Math.Max(1, upperBound));
                    Logger.Trace(String.Format("mutation gene quantity is {0}", mutationGeneQuantity));
                    for (int y = 0; y < mutationGeneQuantity; y++)
                    {
                        int displacedGeneIndex = rng.Next(mutateLocations.Count);
                        Location displacedGene = mutateLocations[displacedGeneIndex];
                        mutateLocations.RemoveAt(displacedGeneIndex);

                        int insertGeneIndex = rng.Next(mutateLocations.Count);
                        /*double proximalInsertionChance = .5;

                        //Have a percent chance of the mutation occurring at a neighbor location of the displaced gene instead of randomly
                        if (rng.Next(101) <= proximalInsertionChance * 100)
                        {
                            Location insertGeneNeighbor = displacedGene.neighbors[displacedGene.neighbors.Count / 5];
                            insertGeneIndex = mutateLocations.IndexOf(insertGeneNeighbor);
                        }*/

                        mutateLocations.Insert(insertGeneIndex, displacedGene);

                    }
                }
            }

            Logger.Trace(string.Format("Returning {0} mutated locations", mutateLocationsList.Count));
            return mutateLocationsList;
        }

        public List<RouteCalculator> runTournament(List<RouteCalculator> contestants)
        {
            Logger.Trace(string.Format("Running tournament with {0} contestants", contestants.Count));



            contestants.SortByDistanceAsc();
            List<RouteCalculator> winners = contestants.Take(tournamentWinnerCount).ToList();

            Logger.Trace(string.Format("Tournament produced {0} winners", contestants.Count));
            foreach (RouteCalculator contestant in contestants)
            {
                Logger.Trace(string.Format("Contestant winner had distance of {0} miles", contestant.metadata.routesLengthMiles));
            }

            //foreach (RouteCalculator contestant in contestants)
            //logLocations(contestant.metadata.processedLocations);
            return winners;
        }

        protected int generateRouteHash(List<Location> locations)
        {
            int hash = 0;
            try
            {
                List<Location> locationsCopy = new List<Location>(locations);
                string concat = "";
                locationsCopy.Sort((a, b) => a.address.CompareTo(b.address));
                foreach (Location location in locationsCopy)
                {
                    concat += location.address;
                }

                hash = concat.GetHashCode();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return hash;
        }

        protected double decayFunction()
        {
            if (toggleIterationsExponent == true)
            {
                double decayPercent = (((double)iterations - (double)currentIteration) / (double)iterations);
                decayPercent = Math.Pow(decayPercent, (double)growthDecayExponent);
                return decayPercent;
            }
            else
                return 1;
        }

        protected double growthFunction()
        {
            if (toggleIterationsExponent == true)
            {
                double growthPercent = (double)currentIteration / (double)iterations;
                growthPercent = Math.Pow(growthPercent, (double)1 / growthDecayExponent);
                return growthPercent;
            }
            else
                return 1;
        }
    }
}
