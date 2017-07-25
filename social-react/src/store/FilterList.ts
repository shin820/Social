import { Reducer } from 'redux';

export class FilterListItem {
    name: string;
    unReadNum: number;
    id: number;
    constructor(id: number, name: string, unReadNum: number) {
        this.id = id;
        this.name = name;
        this.unReadNum = unReadNum;
    }
}

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface FilterListState {
    list: FilterListItem[];
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface ReloadAction { type: 'RELOAD_LIST' }

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).

type KnownAction = ReloadAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    reload: () => <ReloadAction>{ type: 'RELOAD_LIST' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
export const reducer: Reducer<FilterListState> = (state: FilterListState, action: KnownAction) => {
    switch (action.type) {
        case 'RELOAD_LIST':
            return { list: new Array<FilterListItem>() };
        // default:
        //     // The following line guarantees that every action in the KnownAction union has been covered by a case above
        //     const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || { count: 0 };
};