Run the backend
Open a terminal and navigate to the src folder:
cd PlanManager\src
dotnet restore
dotnet run

What happens on first run
EF Core automatically creates plans.db in the src folder and applies the InitialCreate migration, creating the Plans and Steps tables. You will see '>>> DB path: ...' printed in the console confirming the database location.

The backend is now running at:
•	API endpoint:  http://localhost:5000/graphql
•	Banana Cake Pop (GraphQL IDE):  http://localhost:5000/graphql  (open in browser)

Run the frontend
Open a second terminal:
cd PlanManager\frontend
npm install
npm run dev

The Vue app is now running at http://localhost:5173
Note on proxying
Vite proxies all requests from /graphql to http://localhost:5000/graphql including WebSocket upgrades. You do not need to configure CORS or change any URLs when running both servers locally.

Verify everything works
1.	Open http://localhost:5173 in a browser — you should see the PlanManager UI
2.	Create a plan using the form on the left
3.	Expand the plan card and add a step
4.	Open a second browser tab at the same URL and subscribe to plan updates — changes appear in real time
5.	Open http://localhost:5000/graphql to explore the schema in Banana Cake Pop
