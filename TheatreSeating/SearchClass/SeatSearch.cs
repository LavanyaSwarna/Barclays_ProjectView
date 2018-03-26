using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheaterInfo;

namespace SearchService
{

    public class SeatSearch
    {

        //Method to Create List of Requests Object from raw strings data
        public List<Requests> getTicketRequests(string requestsdata)
        {
            List<Requests> requestsList = new List<Requests>();
            Requests request;
            string[] requests = requestsdata.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string data in requests)
            {
                string[] reqList = data.Split(' ');
                request = new Requests();
                request.CustomerName = reqList[0];

                try
                {
                    request.NoOfTickets = Convert.ToInt32(reqList[1]);
                }
                catch (System.FormatException)
                {
                    throw new System.FormatException("'" + reqList[1] + "'" + " is invalid ticket request. Please correct it.");
                }
                request.VerifyComplete = false;
                requestsList.Add(request);
            }
            return requestsList;
        }

        //Method to Create Layout structure Object from raw input data
        public Structure getTheaterLayout(string rawLayout)
        {

            Structure theaterLayout = new Structure();
            TheaterSection section;
            List<TheaterSection> sectionsList = new List<TheaterSection>();
            int totalCapacity = 0, value;
            string[] rows = rawLayout.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string[] sections;

            for (int Rnum = 0; Rnum < rows.Length; Rnum++)
            {

                sections = rows[Rnum].Split(' ');
                for (int SecNum = 0; SecNum < sections.Length; SecNum++)
                {
                    try
                    {
                        value = Convert.ToInt32(sections[SecNum]);
                    }
                    catch (System.FormatException)
                    {
                        throw new System.FormatException("'" + sections[SecNum] + "'" + " is invalid section capacity. Please correct it.");
                    }
                    totalCapacity = totalCapacity + value;
                    section = new TheaterSection();
                    section.RowNumber = Rnum + 1;
                    section.SectionNumber = SecNum + 1;
                    section.Seats = value;
                    section.AvailableSeats = value;
                    sectionsList.Add(section);
                }
            }
            theaterLayout.TotalSeats = totalCapacity;
            theaterLayout.AvailableSeats = totalCapacity;
            theaterLayout.SectionList = sectionsList;
            return theaterLayout;

        }

        //Seat Verification
        private int findCompleteRequest(List<Requests> requests, int CompleteSeats, int CurrentIndex)
        {
            int requestNo = -1;
            for (int i = CurrentIndex + 1; i < requests.Count; i++)
            {
                Requests request = requests[i];

                if (!request.VerifyComplete && request.NoOfTickets == CompleteSeats)
                {

                    requestNo = i;
                    break;
                }
            }
            return requestNo;
        }

        //IComparable Interface to Create Comparable constant
        private class SeatCompare : IComparer<TheaterSection>
        {

            public int Compare(TheaterSection x, TheaterSection y)
            {
                //throw new NotImplementedException();
                return x.AvailableSeats.CompareTo(y.AvailableSeats);
            }
        }

        //Method to Find the section based on availability of seats
        private int findSectionByAvailableSeats(List<TheaterSection> sections, int availableSeats)
        {

            int i = 0;
            sections.Sort();
            TheaterSection SearchKey = new TheaterSection();
            SearchKey.AvailableSeats = availableSeats;
            int Scount = sections.BinarySearch(SearchKey, new SeatCompare());

            if (Scount > 0)
            {
                for (i = Scount - 1; i >= 0; i--)
                {

                    TheaterSection s = sections[i];

                    if (s.AvailableSeats != availableSeats)
                    {
                        break;
                    }
                    Scount = i + 1;
                }
            }


            return Scount;
        }

        public virtual void processTicketRequests(Structure layout, List<Requests> requests)
        {

            for (int i = 0; i < requests.Count; i++)
            {

                Requests request = requests[i];
                if (request.VerifyComplete)
                {
                    continue;
                }

                /*
				 * -2 is an indicator when we can't handle the party.
				 */
                if (request.NoOfTickets > layout.AvailableSeats)
                {

                    request.RowNumber = -2;
                    request.SectionNumber = -2;
                    continue;

                }

                List<TheaterSection> sections = layout.SectionList;

                for (int j = 0; j < sections.Count; j++)
                {

                    TheaterSection section = sections[j];

                    if (request.NoOfTickets == section.AvailableSeats)
                    {
                        request.SectionNumber = section.SectionNumber;
                        request.RowNumber = section.RowNumber;
                        layout.AvailableSeats = layout.AvailableSeats - request.NoOfTickets;
                        section.AvailableSeats = section.AvailableSeats - request.NoOfTickets;
                        request.VerifyComplete = true;
                        break;

                    }
                    else if (request.NoOfTickets < section.AvailableSeats)
                    {

                        int requestNo = findCompleteRequest(requests, section.AvailableSeats - request.NoOfTickets, i);

                        if (requestNo != -1)
                        {
                            request.SectionNumber = section.SectionNumber;
                            request.RowNumber = section.RowNumber;
                            layout.AvailableSeats = layout.AvailableSeats - request.NoOfTickets;
                            section.AvailableSeats = section.AvailableSeats - request.NoOfTickets;
                            request.VerifyComplete = true;

                            Requests completeRequest = requests[requestNo];

                            completeRequest.SectionNumber = section.SectionNumber;
                            completeRequest.RowNumber = section.RowNumber;
                            layout.AvailableSeats = layout.AvailableSeats - completeRequest.NoOfTickets;
                            section.AvailableSeats = section.AvailableSeats - completeRequest.NoOfTickets;
                            completeRequest.VerifyComplete = true;

                            break;

                        }
                        else
                        {

                            int sectionNo = findSectionByAvailableSeats(sections, request.NoOfTickets);

                            if (sectionNo >= 0)
                            {

                                TheaterSection perferctSection = sections[sectionNo];

                                request.RowNumber = perferctSection.RowNumber;
                                request.SectionNumber = perferctSection.SectionNumber;
                                perferctSection.AvailableSeats = perferctSection.AvailableSeats - request.NoOfTickets;
                                layout.AvailableSeats = layout.AvailableSeats - request.NoOfTickets;
                                request.VerifyComplete = true;
                                break;

                            }
                            else
                            {

                                request.RowNumber = section.RowNumber;
                                request.SectionNumber = section.SectionNumber;
                                section.AvailableSeats = section.AvailableSeats - request.NoOfTickets;
                                layout.AvailableSeats = layout.AvailableSeats - request.NoOfTickets;
                                request.VerifyComplete = true;
                                break;

                            }

                        }

                    }

                }

                /*
				 * -1 is an indicator when we need to split the party.
				 */
                if (!request.VerifyComplete)
                {

                    request.RowNumber = -1;
                    request.SectionNumber = -1;

                }

            }

            Console.WriteLine("Final Distribution of Seats by Name , Seat .\n");

            foreach (Requests request in requests)
            {

                Console.WriteLine(request.Status);

            }

        }


    }
}
