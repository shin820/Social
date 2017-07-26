import * as React from 'react';
import * as ReactDOM from 'react-dom';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import { Provider } from 'react-redux';
import { createStore } from 'redux';
import reducers from './reducers/reducers';
import FilterList from './containers/FilterList';
import ConversationList from './containers/ConversationList';

const store = createStore(reducers);

ReactDOM.render(
  <Provider store={store}>
    <FilterList />
    <ConversationList />
  </Provider>,
  document.getElementById('root') as HTMLElement
);
registerServiceWorker();
