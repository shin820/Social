import * as React from 'react';
import * as ReactDOM from 'react-dom';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import { Provider } from 'react-redux';
import { createStore } from 'redux';
import { filter } from './reducers/index';
import { ApplicationState } from './types/index';
import FilterList from './containers/FilterList';

const store = createStore<ApplicationState>(filter, { FilterList: [] });

ReactDOM.render(
  <Provider store={store}>
    <FilterList />
  </Provider>,
  document.getElementById('root') as HTMLElement
);
registerServiceWorker();
