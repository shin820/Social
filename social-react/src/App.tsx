import * as React from 'react';
import './App.css';
import { ConnectedRouter } from 'react-router-redux';
import { Provider } from 'react-redux';
import { createBrowserHistory } from '';
import configureStore from './configureStore';
import { ApplicationState }  from './store';
import * as RoutesModule from './routes';
let routes = RoutesModule.routes;

// Create browser history to use in the Redux store
const history = createBrowserHistory();

// Get the application-wide store instance, prepopulating with state from the server where available.
const initialState = (window as any).initialReduxState as ApplicationState;
const store = configureStore(history, initialState);

const logo = require('./logo.svg');

class App extends React.Component<{}, {}> {
  render() {
    return (
      <Provider store={store}>
        <ConnectedRouter history={history} children={routes} />
      </Provider>
    );
  }
}

export default App;
